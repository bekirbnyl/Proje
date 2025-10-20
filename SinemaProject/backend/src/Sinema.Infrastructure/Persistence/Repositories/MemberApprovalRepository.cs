using Microsoft.EntityFrameworkCore;
using Sinema.Domain.Entities;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for member approvals
/// </summary>
public class MemberApprovalRepository : IMemberApprovalRepository
{
    private readonly SinemaDbContext _context;
    private readonly DbSet<MemberApproval> _memberApprovals;

    public MemberApprovalRepository(SinemaDbContext context)
    {
        _context = context;
        _memberApprovals = context.Set<MemberApproval>();
    }

    public async Task<MemberApproval?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _memberApprovals
            .Include(a => a.Member)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<MemberApproval>> GetByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        return await _memberApprovals
            .Where(a => a.MemberId == memberId)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(MemberApproval approval, CancellationToken cancellationToken = default)
    {
        _memberApprovals.Add(approval);
        // Note: Don't call SaveChangesAsync here - let UnitOfWork handle it
        return Task.CompletedTask;
    }

    public Task UpdateAsync(MemberApproval approval, CancellationToken cancellationToken = default)
    {
        _memberApprovals.Update(approval);
        // Note: Don't call SaveChangesAsync here - let UnitOfWork handle it
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<MemberApproval>> GetPendingVipApplicationsAsync(CancellationToken cancellationToken = default)
    {
        return await _memberApprovals
            .Include(a => a.Member)
            .Where(a => a.Reason.StartsWith("VIP_APPLICATION:") && !a.Approved && a.ApprovedBy == null)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
