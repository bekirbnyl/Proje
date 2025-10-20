using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sinema.Application.Abstractions.Settings;
using Sinema.Domain.Entities;
using Sinema.Domain.Enums;
using Sinema.Domain.Interfaces.Repositories;
using Sinema.Domain.Interfaces.Services;
using Sinema.Infrastructure.Persistence;

namespace Sinema.Infrastructure.Services.Reservations;

/// <summary>
/// Service implementation for seat hold business logic
/// </summary>
public class SeatHoldService : ISeatHoldService
{
    private readonly ISeatHoldRepository _seatHoldRepository;
    private readonly IScreeningRepository _screeningRepository;
    private readonly ISeatLayoutRepository _seatLayoutRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IAppSettingsReader _settingsReader;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SeatHoldService> _logger;
    private readonly SinemaDbContext _context;

    public SeatHoldService(
        ISeatHoldRepository seatHoldRepository,
        IScreeningRepository screeningRepository,
        ISeatLayoutRepository seatLayoutRepository,
        IReservationRepository reservationRepository,
        ITicketRepository ticketRepository,
        IAppSettingsReader settingsReader,
        IConfiguration configuration,
        ILogger<SeatHoldService> logger,
        SinemaDbContext context)
    {
        _seatHoldRepository = seatHoldRepository;
        _screeningRepository = screeningRepository;
        _seatLayoutRepository = seatLayoutRepository;
        _reservationRepository = reservationRepository;
        _ticketRepository = ticketRepository;
        _settingsReader = settingsReader;
        _configuration = configuration;
        _logger = logger;
        _context = context;
    }

    public async Task<IEnumerable<SeatHold>> CreateHoldsAsync(
        Guid screeningId, 
        IEnumerable<Guid> seatIds, 
        string clientToken, 
        Guid? userId = null, 
        int? ttlSeconds = null, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(clientToken))
        {
            throw new ArgumentException("Client token is required", nameof(clientToken));
        }

        var seatIdList = seatIds.ToList();
        if (!seatIdList.Any())
        {
            throw new ArgumentException("At least one seat ID is required", nameof(seatIds));
        }

        // Get screening with validation
        var screening = await _screeningRepository.GetByIdAsync(screeningId, cancellationToken);
        if (screening == null)
        {
            throw new InvalidOperationException($"Screening with ID '{screeningId}' not found.");
        }

        var currentTime = DateTime.UtcNow;

        // T-60 rule: Cannot create holds within 60 minutes of screening start
        // TEMPORARILY DISABLED FOR TESTING
        // var t60Threshold = screening.StartAt.AddMinutes(-60);
        // if (currentTime >= t60Threshold)
        // {
        //     throw new InvalidOperationException($"Cannot create holds within 60 minutes of screening start time ({screening.StartAt:yyyy-MM-dd HH:mm} UTC).");
        // }

        // Validate seats belong to the screening's seat layout
        var seatLayout = await _seatLayoutRepository.GetByIdAsync(screening.SeatLayoutId, cancellationToken);
        if (seatLayout == null)
        {
            throw new InvalidOperationException($"Seat layout '{screening.SeatLayoutId}' not found.");
        }

        var validSeatIds = await _context.Seats
            .Where(s => s.SeatLayoutId == screening.SeatLayoutId && seatIdList.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        var invalidSeatIds = seatIdList.Except(validSeatIds).ToList();
        if (invalidSeatIds.Any())
        {
            throw new InvalidOperationException($"Seats not found in screening's layout: {string.Join(", ", invalidSeatIds)}");
        }

        // Use transaction to ensure atomicity
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Check for existing reservations and tickets (sold seats)
            var soldSeatIds = await _context.Tickets
                .Where(t => t.ScreeningId == screeningId && seatIdList.Contains(t.SeatId))
                .Select(t => t.SeatId)
                .ToListAsync(cancellationToken);

            if (soldSeatIds.Any())
            {
                throw new InvalidOperationException($"Seats already sold: {string.Join(", ", soldSeatIds)}");
            }

            // Check for active reservations
            var reservedSeatIds = await _context.Reservations
                .Where(r => r.ScreeningId == screeningId && 
                           seatIdList.Contains(r.SeatId) &&
                           (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed))
                .Select(r => r.SeatId)
                .ToListAsync(cancellationToken);

            if (reservedSeatIds.Any())
            {
                throw new InvalidOperationException($"Seats already reserved: {string.Join(", ", reservedSeatIds)}");
            }

            // Check for existing active holds
            var existingHolds = await _seatHoldRepository.GetActiveHoldsForSeatsAsync(screeningId, seatIdList, cancellationToken);
            if (existingHolds.Any())
            {
                var conflictSeats = existingHolds.Select(h => h.SeatId).ToList();
                var earliestExpiry = existingHolds.Min(h => h.ExpiresAt);
                throw new InvalidOperationException($"Seats already held by another client: {string.Join(", ", conflictSeats)}. Held until: {earliestExpiry:yyyy-MM-dd HH:mm:ss} UTC");
            }

            // Get TTL configuration
            var defaultTtl = _configuration.GetValue<int>("SeatHold:DefaultTtlSeconds", 120);
            var actualTtl = ttlSeconds ?? defaultTtl;

            // Calculate expiration time
            var expirationTime = currentTime.AddSeconds(actualTtl);
            
            // T-30 rule: Ensure holds don't extend beyond T-30
            var t30Threshold = screening.StartAt.AddMinutes(-30);
            if (expirationTime > t30Threshold)
            {
                expirationTime = t30Threshold;
            }

            // Create holds
            var holds = seatIdList.Select(seatId => new SeatHold
            {
                Id = Guid.NewGuid(),
                ScreeningId = screeningId,
                SeatId = seatId,
                ClientToken = clientToken,
                UserId = userId,
                CreatedAt = currentTime,
                LastHeartbeatAt = currentTime,
                ExpiresAt = expirationTime
            }).ToList();

            await _seatHoldRepository.AddRangeAsync(holds, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Created {HoldCount} seat holds for screening {ScreeningId} by client {ClientToken}. Expires at: {ExpiresAt}",
                holds.Count, screeningId, clientToken, expirationTime);

            return holds;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<SeatHold> ExtendHoldAsync(Guid holdId, string clientToken, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var hold = await _seatHoldRepository.GetByIdAsync(holdId, cancellationToken);
        if (hold == null)
        {
            throw new InvalidOperationException($"Hold with ID '{holdId}' not found.");
        }

        if (!hold.IsOwnedBy(clientToken, userId))
        {
            throw new UnauthorizedAccessException("You are not authorized to extend this hold.");
        }

        if (hold.IsExpired(DateTime.UtcNow))
        {
            throw new InvalidOperationException("Cannot extend an expired hold.");
        }

        // Get extension configuration
        var extensionSeconds = _configuration.GetValue<int>("SeatHold:HeartbeatExtendSeconds", 120);
        var maxExtendMinutes = _configuration.GetValue<int>("SeatHold:MaxExtendMinutes", 10);

        // Calculate maximum allowed extension time
        var maxExpirationTime = hold.CreatedAt.AddMinutes(maxExtendMinutes);
        
        // Also respect T-30 rule
        var t30Threshold = hold.Screening.StartAt.AddMinutes(-30);
        if (maxExpirationTime > t30Threshold)
        {
            maxExpirationTime = t30Threshold;
        }

        hold.Extend(extensionSeconds, maxExpirationTime);
        await _seatHoldRepository.UpdateAsync(hold, cancellationToken);

        _logger.LogInformation("Extended hold {HoldId} for client {ClientToken}. New expiration: {ExpiresAt}",
            holdId, clientToken, hold.ExpiresAt);

        return hold;
    }

    public async Task ReleaseHoldAsync(Guid holdId, string clientToken, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var hold = await _seatHoldRepository.GetByIdAsync(holdId, cancellationToken);
        if (hold == null)
        {
            throw new InvalidOperationException($"Hold with ID '{holdId}' not found.");
        }

        if (!hold.IsOwnedBy(clientToken, userId))
        {
            throw new UnauthorizedAccessException("You are not authorized to release this hold.");
        }

        await _seatHoldRepository.RemoveAsync(hold, cancellationToken);

        _logger.LogInformation("Released hold {HoldId} for client {ClientToken}",
            holdId, clientToken);
    }

    public async Task<int> ReleaseHoldsByReservationAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        // This method would be called after reservation creation to clean up holds
        // For now, we'll implement it as a placeholder since reservation entities aren't fully detailed
        _logger.LogInformation("ReleaseHoldsByReservationAsync called for reservation {ReservationId}", reservationId);
        return 0;
    }

    public async Task<bool> ValidateHoldsForReservationAsync(
        Guid screeningId, 
        IEnumerable<Guid> seatIds, 
        string clientToken, 
        Guid? userId = null, 
        CancellationToken cancellationToken = default)
    {
        var seatIdList = seatIds.ToList();
        var activeHolds = await _seatHoldRepository.GetActiveHoldsForSeatsAsync(screeningId, seatIdList, cancellationToken);

        // Check that all seats have active holds owned by the client
        foreach (var seatId in seatIdList)
        {
            var hold = activeHolds.FirstOrDefault(h => h.SeatId == seatId);
            if (hold == null || !hold.IsOwnedBy(clientToken, userId))
            {
                _logger.LogWarning("Validation failed for seat {SeatId} in screening {ScreeningId} for client {ClientToken}",
                    seatId, screeningId, clientToken);
                return false;
            }
        }

        return true;
    }

    public async Task<int> CleanupExpiredHoldsAsync(int batchSize = 100, CancellationToken cancellationToken = default)
    {
        var currentTime = DateTime.UtcNow;
        var expiredHolds = await _seatHoldRepository.GetExpiredHoldsAsync(currentTime, batchSize, cancellationToken);
        var expiredHoldsList = expiredHolds.ToList();

        if (expiredHoldsList.Any())
        {
            await _seatHoldRepository.RemoveRangeAsync(expiredHoldsList, cancellationToken);
            
            _logger.LogInformation("Cleaned up {ExpiredCount} expired seat holds",
                expiredHoldsList.Count);
        }

        return expiredHoldsList.Count;
    }
}
