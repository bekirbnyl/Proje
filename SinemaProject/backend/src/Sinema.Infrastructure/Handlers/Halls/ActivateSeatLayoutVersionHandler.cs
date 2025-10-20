using MediatR;
using Microsoft.EntityFrameworkCore;
using Sinema.Application.Features.Halls.Commands.ActivateSeatLayoutVersion;
using Sinema.Infrastructure.Persistence;

namespace Sinema.Infrastructure.Handlers.Halls;

/// <summary>
/// Handler for activating a seat layout version
/// </summary>
public class ActivateSeatLayoutVersionHandler : IRequestHandler<ActivateSeatLayoutVersionCommand, bool>
{
    private readonly SinemaDbContext _context;

    public ActivateSeatLayoutVersionHandler(SinemaDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ActivateSeatLayoutVersionCommand request, CancellationToken cancellationToken)
    {
        // Verify the layout exists and belongs to the hall
        var targetLayout = await _context.SeatLayouts
            .FirstOrDefaultAsync(sl => sl.Id == request.LayoutId && sl.HallId == request.HallId, cancellationToken);

        if (targetLayout == null)
        {
            throw new ArgumentException($"Seat layout with ID {request.LayoutId} not found for hall {request.HallId}.", nameof(request.LayoutId));
        }

        // If already active, this is idempotent - return true
        if (targetLayout.IsActive)
        {
            return true;
        }

        // Start transaction to ensure atomicity
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Deactivate all other layouts for this hall
            var hallLayouts = await _context.SeatLayouts
                .Where(sl => sl.HallId == request.HallId && sl.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var layout in hallLayouts)
            {
                layout.IsActive = false;
            }

            // Activate the target layout
            targetLayout.IsActive = true;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return true;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
