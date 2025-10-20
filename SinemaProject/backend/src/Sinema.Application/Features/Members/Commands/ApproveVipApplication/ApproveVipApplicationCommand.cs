using MediatR;

namespace Sinema.Application.Features.Members.Commands.ApproveVipApplication;

/// <summary>
/// Command for approving a VIP application
/// </summary>
public class ApproveVipApplicationCommand : IRequest<bool>
{
    /// <summary>
    /// The member approval ID (VIP application)
    /// </summary>
    public Guid ApprovalId { get; set; }

    /// <summary>
    /// User who is approving the VIP application
    /// </summary>
    public Guid? ApprovedBy { get; set; }

    /// <summary>
    /// Reason for approval
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
