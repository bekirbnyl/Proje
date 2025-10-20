using Microsoft.EntityFrameworkCore;
using Sinema.Domain.Entities;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Member entity
/// </summary>
public class MemberRepository : IMemberRepository
{
    private readonly SinemaDbContext _context;

    public MemberRepository(SinemaDbContext context)
    {
        _context = context;
    }

    public async Task<Member?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Members
            .Include(m => m.Approvals)
            .Include(m => m.Credits)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<Member?> GetByIdWithoutRelationsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Members
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<Member?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Members
            .Include(m => m.Approvals)
            .Include(m => m.Credits)
            .FirstOrDefaultAsync(m => m.Email == email, cancellationToken);
    }

    public async Task<IEnumerable<Member>> GetVipMembersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Members
            .Include(m => m.Approvals)
            .Include(m => m.Credits)
            .Where(m => m.VipStatus)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Member>> GetApprovedMembersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Members
            .Include(m => m.Approvals)
            .Include(m => m.Credits)
            .Where(m => m.Approvals.Any(a => a.Approved))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Member>> GetPendingApprovalMembersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Members
            .Include(m => m.Approvals)
            .Include(m => m.Credits)
            .Where(m => !m.Approvals.Any(a => a.Approved))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Member member, CancellationToken cancellationToken = default)
    {
        member.CreatedAt = DateTime.UtcNow;
        _context.Members.Add(member);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(Member member, CancellationToken cancellationToken = default)
    {
        member.UpdatedAt = DateTime.UtcNow;
        _context.Members.Update(member);
        // Note: Don't call SaveChangesAsync here - let UnitOfWork handle it
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Member member, CancellationToken cancellationToken = default)
    {
        _context.Members.Remove(member);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<decimal> GetAvailableCreditAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        var member = await _context.Members
            .Include(m => m.Credits)
            .FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);

        if (member == null)
        {
            return 0m;
        }

        return member.Credits?.Sum(c => c.Amount) ?? 0m;
    }
}
