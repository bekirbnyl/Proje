using Sinema.Domain.Entities;

namespace Sinema.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for member approvals
/// </summary>
public interface IMemberApprovalRepository
{
    /// <summary>
    /// Get approval by ID
    /// </summary>
    Task<MemberApproval?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get approvals by member ID
    /// </summary>
    Task<IEnumerable<MemberApproval>> GetByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Add a new approval
    /// </summary>
    Task AddAsync(MemberApproval approval, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update an approval
    /// </summary>
    Task UpdateAsync(MemberApproval approval, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all pending VIP applications
    /// </summary>
    Task<IEnumerable<MemberApproval>> GetPendingVipApplicationsAsync(CancellationToken cancellationToken = default);
}
