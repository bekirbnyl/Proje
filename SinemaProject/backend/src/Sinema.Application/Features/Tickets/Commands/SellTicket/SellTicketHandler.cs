using MediatR;
using Microsoft.Extensions.Logging;
using Sinema.Application.Abstractions.Idempotency;
using Sinema.Application.Abstractions.Payments;
using Sinema.Application.Abstractions.Pricing;
using Sinema.Application.Abstractions.Tickets;
using Sinema.Application.DTOs.Pricing;
using Sinema.Application.DTOs.Tickets;
using Sinema.Domain.Entities;
using Sinema.Domain.Enums;
using Sinema.Domain.Interfaces.Repositories;
using Sinema.Domain.Interfaces.Services;
using System.Text.Json;

namespace Sinema.Application.Features.Tickets.Commands.SellTicket;

/// <summary>
/// Handler for selling tickets with comprehensive business rules and payment processing
/// </summary>
public class SellTicketHandler : IRequestHandler<SellTicketCommand, SellTicketResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITicketRepository _ticketRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IScreeningRepository _screeningRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly ISeatHoldRepository _seatHoldRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly ISeatHoldService _seatHoldService;
    private readonly IPricingEngine _pricingEngine;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ITicketNumberGenerator _ticketNumberGenerator;
    private readonly IIdempotencyStore _idempotencyStore;
    private readonly ILogger<SellTicketHandler> _logger;

    public SellTicketHandler(
        IUnitOfWork unitOfWork,
        ITicketRepository ticketRepository,
        IReservationRepository reservationRepository,
        IScreeningRepository screeningRepository,
        ISeatRepository seatRepository,
        ISeatHoldRepository seatHoldRepository,
        IPaymentRepository paymentRepository,
        IMemberRepository memberRepository,
        ISeatHoldService seatHoldService,
        IPricingEngine pricingEngine,
        IPaymentGateway paymentGateway,
        ITicketNumberGenerator ticketNumberGenerator,
        IIdempotencyStore idempotencyStore,
        ILogger<SellTicketHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _ticketRepository = ticketRepository;
        _reservationRepository = reservationRepository;
        _screeningRepository = screeningRepository;
        _seatRepository = seatRepository;
        _seatHoldRepository = seatHoldRepository;
        _paymentRepository = paymentRepository;
        _memberRepository = memberRepository;
        _seatHoldService = seatHoldService;
        _pricingEngine = pricingEngine;
        _paymentGateway = paymentGateway;
        _ticketNumberGenerator = ticketNumberGenerator;
        _idempotencyStore = idempotencyStore;
        _logger = logger;
    }

    public async Task<SellTicketResponse> Handle(SellTicketCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        // Check idempotency first
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existingResult = await _idempotencyStore.GetResultAsync<SellTicketResponse>(request.IdempotencyKey);
            if (existingResult != null)
            {
                _logger.LogInformation("Returning cached result for idempotency key: {Key}", request.IdempotencyKey);
                return existingResult;
            }
        }

        // Validate screening exists and is not started
        var screening = await _screeningRepository.GetByIdAsync(request.ScreeningId, cancellationToken);
        if (screening == null)
        {
            throw new InvalidOperationException($"Screening with ID '{request.ScreeningId}' not found");
        }

        var currentTime = DateTime.UtcNow;
        if (currentTime >= screening.StartAt)
        {
            throw new InvalidOperationException("Cannot sell tickets for screenings that have already started");
        }

        // Determine sale mode and get seat information
        List<SeatInfo> seatInfos;
        Reservation? reservation = null;

        if (request.ReservationId.HasValue)
        {
            // Reservation-based sale
            (seatInfos, reservation) = await GetSeatsFromReservationAsync(request, cancellationToken);
        }
        else
        {
            // Direct sale
            seatInfos = await GetSeatsDirectAsync(request, cancellationToken);
        }

        // Validate member if specified
        Member? member = null;
        if (request.MemberId.HasValue)
        {
            member = await _memberRepository.GetByIdAsync(request.MemberId.Value, cancellationToken);
            if (member == null)
            {
                throw new InvalidOperationException($"Member with ID '{request.MemberId}' not found");
            }
        }

        // Calculate pricing using the pricing engine
        var priceQuote = await CalculatePricingAsync(request, seatInfos, member, cancellationToken);

        // Process payment
        var paymentMetadata = new Dictionary<string, string>
        {
            ["screening_id"] = request.ScreeningId.ToString(),
            ["seat_count"] = seatInfos.Count.ToString(),
            ["channel"] = request.Channel.ToString()
        };

        if (request.ReservationId.HasValue)
        {
            paymentMetadata["reservation_id"] = request.ReservationId.Value.ToString();
        }

        var paymentResult = await _paymentGateway.AuthorizeAndCaptureAsync(
            priceQuote.TotalAfter,
            request.PaymentMethod,
            request.MemberId,
            paymentMetadata);

        if (!paymentResult.IsSuccess)
        {
            _logger.LogWarning("Payment failed for screening {ScreeningId}: {Error}", 
                request.ScreeningId, paymentResult.ErrorMessage);
            
            return new SellTicketResponse
            {
                PaymentStatus = PaymentStatus.Failed,
                TotalBefore = priceQuote.TotalBefore,
                TotalAfter = priceQuote.TotalAfter,
                Items = new List<TicketItemResponse>()
            };
        }

        // Create tickets in transaction
        var response = await CreateTicketsTransactionAsync(
            request, seatInfos, priceQuote, paymentResult, reservation, cancellationToken);

        // Store result for idempotency
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            await _idempotencyStore.StoreResultAsync(request.IdempotencyKey, response);
        }

        return response;
    }

    private async Task<(List<SeatInfo>, Reservation?)> GetSeatsFromReservationAsync(
        SellTicketRequest request, CancellationToken cancellationToken)
    {
        var reservations = await _reservationRepository.GetByReservationIdAsync(
            request.ReservationId!.Value, cancellationToken);

        if (!reservations.Any())
        {
            throw new InvalidOperationException($"Reservation with ID '{request.ReservationId}' not found");
        }

        var firstReservation = reservations.First();

        // Validate reservation belongs to member (if specified)
        if (request.MemberId.HasValue && firstReservation.MemberId != request.MemberId)
        {
            throw new InvalidOperationException("Reservation does not belong to the specified member");
        }

        // Validate reservation status
        if (firstReservation.Status != ReservationStatus.Pending && firstReservation.Status != ReservationStatus.Confirmed)
        {
            throw new InvalidOperationException($"Reservation status '{firstReservation.Status}' is not valid for purchase");
        }

        // Validate all reservations are for the same screening
        if (reservations.Any(r => r.ScreeningId != request.ScreeningId))
        {
            throw new InvalidOperationException("All reservations must be for the same screening");
        }

        // Validate seat holds if client token provided
        if (!string.IsNullOrWhiteSpace(request.ClientToken))
        {
            var seatIds = reservations.Select(r => r.SeatId).ToList();
            var isValidHolds = await _seatHoldService.ValidateHoldsForReservationAsync(
                request.ScreeningId, seatIds, request.ClientToken, request.MemberId, cancellationToken);

            if (!isValidHolds)
            {
                throw new InvalidOperationException("Seat holds are not valid for this transaction");
            }
        }

        var seatInfos = new List<SeatInfo>();
        foreach (var res in reservations)
        {
            var seat = await _seatRepository.GetByIdAsync(res.SeatId, cancellationToken);
            if (seat == null)
            {
                throw new InvalidOperationException($"Seat with ID '{res.SeatId}' not found");
            }

            seatInfos.Add(new SeatInfo
            {
                SeatId = res.SeatId,
                Seat = seat,
                TicketType = TicketType.Full, // Default, will be determined by pricing engine
                IsVipGuest = false
            });
        }

        return (seatInfos, firstReservation);
    }

    private async Task<List<SeatInfo>> GetSeatsDirectAsync(
        SellTicketRequest request, CancellationToken cancellationToken)
    {
        if (request.Items == null || !request.Items.Any())
        {
            throw new InvalidOperationException("Items are required for direct sales");
        }

        var seatInfos = new List<SeatInfo>();
        var seatIds = request.Items.Select(i => i.SeatId).ToList();

        // Check for duplicates
        if (seatIds.Count != seatIds.Distinct().Count())
        {
            throw new InvalidOperationException("Duplicate seats are not allowed");
        }

        // Validate seats exist
        foreach (var item in request.Items)
        {
            var seat = await _seatRepository.GetByIdAsync(item.SeatId, cancellationToken);
            if (seat == null)
            {
                throw new InvalidOperationException($"Seat with ID '{item.SeatId}' not found");
            }

            // Check if seat is already sold
            var existingTicket = await _ticketRepository.GetByScreeningAndSeatAsync(
                request.ScreeningId, item.SeatId, cancellationToken);
            if (existingTicket != null)
            {
                throw new InvalidOperationException($"Seat {seat.Label} is already sold");
            }

            // Check for conflicting holds (only for non-box office sales)
            if (request.Channel != TicketChannel.BoxOffice)
            {
                var conflictingHolds = await _seatHoldRepository.GetActiveHoldsBySeatAsync(
                    request.ScreeningId, item.SeatId, cancellationToken);

                if (conflictingHolds.Any())
                {
                    var conflictingHold = conflictingHolds.First();
                    if (string.IsNullOrWhiteSpace(request.ClientToken) || 
                        conflictingHold.ClientToken != request.ClientToken)
                    {
                        throw new InvalidOperationException($"Seat {seat.Label} is held by another user");
                    }
                }
            }

            seatInfos.Add(new SeatInfo
            {
                SeatId = item.SeatId,
                Seat = seat,
                TicketType = item.TicketType,
                IsVipGuest = item.IsVipGuest
            });
        }

        return seatInfos;
    }

    private async Task<PriceQuoteResponse> CalculatePricingAsync(
        SellTicketRequest request, List<SeatInfo> seatInfos, Member? member, CancellationToken cancellationToken)
    {
        var quoteItems = seatInfos.Select(si => new QuoteItemRequest
        {
            SeatId = si.SeatId,
            TicketType = si.TicketType,
            IsVipGuest = si.IsVipGuest
        }).ToList();

        var priceQuoteRequest = new PriceQuoteRequest
        {
            ScreeningId = request.ScreeningId,
            MemberId = request.MemberId,
            Items = quoteItems
        };

        return await _pricingEngine.CalculateQuoteAsync(priceQuoteRequest);
    }

    private async Task<SellTicketResponse> CreateTicketsTransactionAsync(
        SellTicketRequest request,
        List<SeatInfo> seatInfos,
        PriceQuoteResponse priceQuote,
        PaymentResult paymentResult,
        Reservation? reservation,
        CancellationToken cancellationToken)
    {
        // Start transaction
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // Create payment record
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                Amount = priceQuote.TotalAfter,
                Method = request.PaymentMethod,
                Status = PaymentStatus.Succeeded,
                ExternalReference = paymentResult.TransactionId,
                CreatedAt = DateTime.UtcNow
            };

            await _paymentRepository.AddAsync(payment, cancellationToken);

            // Create tickets
            var ticketItems = new List<TicketItemResponse>();
            
            for (int i = 0; i < seatInfos.Count; i++)
            {
                var seatInfo = seatInfos[i];
                var quoteItem = priceQuote.Items[i];

                var ticket = new Ticket
                {
                    Id = Guid.NewGuid(),
                    ScreeningId = request.ScreeningId,
                    SeatId = seatInfo.SeatId,
                    Type = seatInfo.TicketType,
                    Channel = request.Channel,
                    Price = quoteItem.FinalPrice,
                    PaymentId = payment.Id,
                    SoldAt = DateTime.UtcNow,
                    TicketCode = await GenerateUniqueTicketCodeAsync(cancellationToken),
                    AppliedPricingJson = JsonSerializer.Serialize(quoteItem)
                };

                await _ticketRepository.AddAsync(ticket, cancellationToken);

                ticketItems.Add(new TicketItemResponse
                {
                    TicketId = ticket.Id,
                    TicketCode = ticket.TicketCode,
                    SeatId = ticket.SeatId,
                    FinalPrice = ticket.Price,
                    AppliedRule = quoteItem.AppliedRule
                });
            }

            // Handle reservation completion
            if (reservation != null)
            {
                var allReservations = await _reservationRepository.GetByReservationIdAsync(
                    reservation.Id, cancellationToken);

                foreach (var res in allReservations)
                {
                    res.Status = ReservationStatus.Completed;
                    await _reservationRepository.UpdateAsync(res, cancellationToken);
                }

                // Clean up seat holds
                var seatIds = allReservations.Select(r => r.SeatId).ToList();
                await _seatHoldRepository.RemoveHoldsBySeatsAsync(
                    request.ScreeningId, seatIds, request.ClientToken, request.MemberId, cancellationToken);
            }
            else if (!string.IsNullOrWhiteSpace(request.ClientToken))
            {
                // Clean up holds for direct sales
                var seatIds = seatInfos.Select(si => si.SeatId).ToList();
                await _seatHoldRepository.RemoveHoldsBySeatsAsync(
                    request.ScreeningId, seatIds, request.ClientToken, request.MemberId, cancellationToken);
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Successfully sold {TicketCount} tickets for screening {ScreeningId}, total amount: {Amount}",
                ticketItems.Count, request.ScreeningId, payment.Amount);

            return new SellTicketResponse
            {
                PaymentStatus = PaymentStatus.Succeeded,
                TotalBefore = priceQuote.TotalBefore,
                TotalAfter = priceQuote.TotalAfter,
                Items = ticketItems,
                PaymentReference = paymentResult.TransactionId
            };
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Failed to create tickets for screening {ScreeningId}", request.ScreeningId);
            throw;
        }
    }

    private async Task<string> GenerateUniqueTicketCodeAsync(CancellationToken cancellationToken)
    {
        string ticketCode;
        bool isUnique;
        int attempts = 0;
        const int maxAttempts = 10;

        do
        {
            ticketCode = _ticketNumberGenerator.GenerateTicketCode();
            isUnique = !await _ticketRepository.ExistsByTicketCodeAsync(ticketCode, cancellationToken);
            attempts++;
        }
        while (!isUnique && attempts < maxAttempts);

        if (!isUnique)
        {
            throw new InvalidOperationException("Unable to generate unique ticket code after multiple attempts");
        }

        return ticketCode;
    }

    private class SeatInfo
    {
        public Guid SeatId { get; set; }
        public Seat Seat { get; set; } = null!;
        public TicketType TicketType { get; set; }
        public bool IsVipGuest { get; set; }
    }
}
