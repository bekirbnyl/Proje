using MediatR;

namespace Sinema.Application.Features.Members.Commands.ApplyForVip;

/// <summary>
/// Command for applying for VIP status
/// </summary>
public class ApplyForVipCommand : IRequest<bool>
{
    /// <summary>
    /// Member ID who is applying for VIP status
    /// </summary>
    public Guid MemberId { get; set; }

    /// <summary>
    /// Reason for VIP application
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// User ID who is applying (for tracking)
    /// </summary>
    public string UserId { get; set; } = string.Empty;
}
