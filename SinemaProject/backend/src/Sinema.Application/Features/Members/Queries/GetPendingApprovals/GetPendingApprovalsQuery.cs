using MediatR;
using Sinema.Application.DTOs.Members;

namespace Sinema.Application.Features.Members.Queries.GetPendingApprovals;

/// <summary>
/// Query to get all pending member approvals
/// </summary>
public record GetPendingApprovalsQuery : IRequest<IEnumerable<MemberApprovalDto>>
{
}
