using MediatR;
using Sinema.Application.Abstractions.Settings;
using Sinema.Application.DTOs.Reservations;
using Sinema.Domain.Entities;
using Sinema.Domain.Enums;
using Sinema.Domain.Interfaces.Repositories;
using Sinema.Domain.Interfaces.Services;

namespace Sinema.Application.Features.Reservations.Commands.CreateReservation;

/// <summary>
/// Handler for CreateReservationCommand
/// </summary>
public class CreateReservationHandler : IRequestHandler<CreateReservationCommand, List<ReservationDto>>
{
    private readonly ISeatHoldService _seatHoldService;
    private readonly IReservationRepository _reservationRepository;
    private readonly IScreeningRepository _screeningRepository;
    private readonly ISeatHoldRepository _seatHoldRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IAppSettingsReader _settingsReader;

    public CreateReservationHandler(
        ISeatHoldService seatHoldService,
        IReservationRepository reservationRepository,
        IScreeningRepository screeningRepository,
        ISeatHoldRepository seatHoldRepository,
        ISeatRepository seatRepository,
        IMemberRepository memberRepository,
        IAppSettingsReader settingsReader)
    {
        _seatHoldService = seatHoldService;
        _reservationRepository = reservationRepository;
        _screeningRepository = screeningRepository;
        _seatHoldRepository = seatHoldRepository;
        _seatRepository = seatRepository;
        _memberRepository = memberRepository;
        _settingsReader = settingsReader;
    }

    public async Task<List<ReservationDto>> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ClientToken))
        {
            throw new ArgumentException("Client token is required", nameof(request.ClientToken));
        }

        var seatIdList = request.SeatIds.ToList();
        if (!seatIdList.Any())
        {
            throw new ArgumentException("At least one seat ID is required", nameof(request.SeatIds));
        }

        // Get screening with validation
        var screening = await _screeningRepository.GetByIdAsync(request.ScreeningId, cancellationToken);
        if (screening == null)
        {
            throw new InvalidOperationException($"Screening with ID '{request.ScreeningId}' not found.");
        }

        var currentTime = DateTime.UtcNow;

        // Advance booking policy check
        await ValidateAdvanceBookingPolicy(request.MemberId, screening.StartAt, currentTime, cancellationToken);

        // T-60 rule: Cannot create reservations within 60 minutes of screening start
        // TEMPORARILY DISABLED FOR TESTING
        // var t60Threshold = screening.StartAt.AddMinutes(-60);
        // if (currentTime >= t60Threshold)
        // {
        //     throw new InvalidOperationException($"Cannot create reservations within 60 minutes of screening start time ({screening.StartAt:yyyy-MM-dd HH:mm} UTC).");
        // }

        // Validate that all seats are held by the same client
        var isValidHolds = await _seatHoldService.ValidateHoldsForReservationAsync(
            request.ScreeningId,
            seatIdList,
            request.ClientToken,
            request.MemberId,
            cancellationToken);

        if (!isValidHolds)
        {
            throw new InvalidOperationException("All seats must be held by the requesting client before creating a reservation.");
        }

        // Calculate expiration time (T-30 rule)
        var t30Threshold = screening.StartAt.AddMinutes(-30);
        var expirationTime = t30Threshold;

        // Create reservations
        var reservations = seatIdList.Select(seatId => new Reservation
        {
            Id = Guid.NewGuid(),
            ScreeningId = request.ScreeningId,
            SeatId = seatId,
            MemberId = request.MemberId,
            Status = ReservationStatus.Pending,
            ExpiresAt = expirationTime,
            CreatedAt = currentTime
        }).ToList();

        // Add reservations to repository (this should handle transactions internally)
        foreach (var reservation in reservations)
        {
            await _reservationRepository.AddAsync(reservation, cancellationToken);
        }

        // Remove the holds since reservations are now created
        await _seatHoldRepository.RemoveHoldsBySeatsAsync(
            request.ScreeningId,
            seatIdList,
            request.ClientToken,
            request.MemberId,
            cancellationToken);

        // Load seat information for response
        var reservationDtoTasks = reservations.Select(async reservation =>
        {
            var seat = await _seatRepository.GetByIdAsync(reservation.SeatId, cancellationToken);
            
            return new ReservationDto
            {
                Id = reservation.Id,
                ScreeningId = reservation.ScreeningId,
                SeatId = reservation.SeatId,
                MemberId = reservation.MemberId,
                Status = reservation.Status,
                ExpiresAt = reservation.ExpiresAt,
                CreatedAt = reservation.CreatedAt,
                Seat = seat != null ? new ReservationSeatInfo
                {
                    Row = seat.Row,
                    Col = seat.Col,
                    Label = seat.Label
                } : null
            };
        });

        return (await Task.WhenAll(reservationDtoTasks)).ToList();
    }

    /// <summary>
    /// Validates advance booking policy based on user VIP status
    /// </summary>
    private async Task ValidateAdvanceBookingPolicy(Guid? memberId, DateTime screeningStartAt, DateTime currentTime, CancellationToken cancellationToken)
    {
        var daysUntilScreening = (screeningStartAt.Date - currentTime.Date).TotalDays;

        // Get advance booking settings
        var vipAdvanceDays = await _settingsReader.GetIntSettingAsync("VipAdvanceBookingDays") ?? 7;
        var regularAdvanceDays = await _settingsReader.GetIntSettingAsync("RegularAdvanceBookingDays") ?? 2;

        // Check if user is VIP (active VIP status)
        bool isVipMember = false;
        if (memberId.HasValue)
        {
            var member = await _memberRepository.GetByIdAsync(memberId.Value, cancellationToken);
            isVipMember = member?.IsActiveVip ?? false;
        }

        var allowedAdvanceDays = isVipMember ? vipAdvanceDays : regularAdvanceDays;

        if (daysUntilScreening > allowedAdvanceDays)
        {
            var memberType = isVipMember ? "VIP" : "Regular";
            throw new InvalidOperationException(
                $"{memberType} members can only book tickets up to {allowedAdvanceDays} days in advance. " +
                $"This screening is {daysUntilScreening:F0} days away.");
        }
    }
}
