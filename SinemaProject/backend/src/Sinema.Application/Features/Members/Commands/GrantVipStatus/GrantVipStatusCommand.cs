using MediatR;

namespace Sinema.Application.Features.Members.Commands.GrantVipStatus;

/// <summary>
/// Command to grant VIP status to a member
/// </summary>
public record GrantVipStatusCommand : IRequest<bool>
{
    /// <summary>
    /// ID of the member to grant VIP status
    /// </summary>
    public Guid MemberId { get; init; }

    /// <summary>
    /// ID of the user granting VIP status
    /// </summary>
    public Guid? GrantedBy { get; init; }

    /// <summary>
    /// VIP membership start date
    /// </summary>
    public DateTime VipStartDate { get; init; }

    /// <summary>
    /// VIP membership end date
    /// </summary>
    public DateTime VipEndDate { get; init; }

    /// <summary>
    /// Payment ID for VIP membership
    /// </summary>
    public Guid? VipPaymentId { get; init; }
}
