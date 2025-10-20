using Microsoft.EntityFrameworkCore;
using Sinema.Application.Abstractions.Pricing;
using Sinema.Domain.Enums;
using Sinema.Infrastructure.Persistence;

namespace Sinema.Infrastructure.Services.Pricing;

/// <summary>
/// Service for tracking VIP member benefit usage
/// </summary>
public class VipUsageService : IVipUsageService
{
    private readonly SinemaDbContext _context;

    public VipUsageService(SinemaDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetVipFreeTicketCountThisMonthAsync(Guid memberId)
    {
        var currentDate = DateTime.UtcNow;
        var startOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);

        var count = await _context.Tickets
            .Where(t => t.Payment != null && 
                       t.Payment.Method != PaymentMethod.MemberCredit && // Exclude member credit payments
                       t.Price == 0 && // Free tickets
                       t.SoldAt >= startOfMonth && 
                       t.SoldAt < endOfMonth)
            .Join(_context.Reservations,
                ticket => new { ticket.ScreeningId, ticket.SeatId },
                reservation => new { reservation.ScreeningId, reservation.SeatId },
                (ticket, reservation) => new { Ticket = ticket, Reservation = reservation })
            .Where(tr => tr.Reservation.MemberId == memberId)
            .CountAsync();

        return count;
    }

    public async Task<bool> HasUsedMonthlyFreeTicketAsync(Guid memberId)
    {
        var freeTicketCount = await GetVipFreeTicketCountThisMonthAsync(memberId);
        return freeTicketCount > 0;
    }
}