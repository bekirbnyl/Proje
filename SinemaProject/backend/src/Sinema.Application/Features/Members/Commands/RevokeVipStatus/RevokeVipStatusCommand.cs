using MediatR;

namespace Sinema.Application.Features.Members.Commands.RevokeVipStatus;

/// <summary>
/// Command to revoke VIP status from a member
/// </summary>
public record RevokeVipStatusCommand : IRequest<bool>
{
    /// <summary>
    /// ID of the member to revoke VIP status
    /// </summary>
    public Guid MemberId { get; init; }

    /// <summary>
    /// ID of the user revoking VIP status
    /// </summary>
    public Guid? RevokedBy { get; init; }
}
