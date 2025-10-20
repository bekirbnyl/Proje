using MediatR;

namespace Sinema.Application.Features.Members.Commands.RejectMember;

/// <summary>
/// Command to reject a member
/// </summary>
public record RejectMemberCommand : IRequest<bool>
{
    /// <summary>
    /// ID of the member to reject
    /// </summary>
    public Guid MemberId { get; init; }

    /// <summary>
    /// Reason for rejection
    /// </summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>
    /// ID of the user rejecting the member
    /// </summary>
    public Guid? RejectedBy { get; init; }
}
