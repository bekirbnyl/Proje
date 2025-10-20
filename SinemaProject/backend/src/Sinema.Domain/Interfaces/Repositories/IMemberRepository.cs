using Sinema.Domain.Entities;

namespace Sinema.Domain.Interfaces.Repositories;

public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Member?> GetByIdWithoutRelationsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Member?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<Member>> GetVipMembersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Member>> GetApprovedMembersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Member>> GetPendingApprovalMembersAsync(CancellationToken cancellationToken = default);
    Task<decimal> GetAvailableCreditAsync(Guid memberId, CancellationToken cancellationToken = default);
    Task AddAsync(Member member, CancellationToken cancellationToken = default);
    Task UpdateAsync(Member member, CancellationToken cancellationToken = default);
    Task DeleteAsync(Member member, CancellationToken cancellationToken = default);
}
