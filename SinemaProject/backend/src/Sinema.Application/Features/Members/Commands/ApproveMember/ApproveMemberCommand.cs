using MediatR;

namespace Sinema.Application.Features.Members.Commands.ApproveMember;

/// <summary>
/// Command to approve a member
/// </summary>
public record ApproveMemberCommand : IRequest<bool>
{
    /// <summary>
    /// ID of the member to approve
    /// </summary>
    public Guid MemberId { get; init; }

    /// <summary>
    /// Reason for approval
    /// </summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>
    /// ID of the user approving the member
    /// </summary>
    public Guid? ApprovedBy { get; init; }
}
