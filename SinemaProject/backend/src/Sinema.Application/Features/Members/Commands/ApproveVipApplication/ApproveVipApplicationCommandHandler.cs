using MediatR;
using Microsoft.Extensions.Logging;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Application.Features.Members.Commands.ApproveVipApplication;

/// <summary>
/// Handler for approving VIP applications
/// </summary>
public class ApproveVipApplicationCommandHandler : IRequestHandler<ApproveVipApplicationCommand, bool>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IMemberApprovalRepository _memberApprovalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ApproveVipApplicationCommandHandler> _logger;

    public ApproveVipApplicationCommandHandler(
        IMemberRepository memberRepository,
        IMemberApprovalRepository memberApprovalRepository,
        IUnitOfWork unitOfWork,
        ILogger<ApproveVipApplicationCommandHandler> logger)
    {
        _memberRepository = memberRepository;
        _memberApprovalRepository = memberApprovalRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(ApproveVipApplicationCommand request, CancellationToken cancellationToken)
    {
        var approval = await _memberApprovalRepository.GetByIdAsync(request.ApprovalId, cancellationToken);
        
        if (approval == null)
        {
            _logger.LogWarning("VIP application not found: {ApprovalId}", request.ApprovalId);
            return false;
        }

        if (!approval.Reason.StartsWith("VIP_APPLICATION:"))
        {
            _logger.LogWarning("Approval {ApprovalId} is not a VIP application", request.ApprovalId);
            return false;
        }

        if (approval.Approved || approval.ApprovedBy != null)
        {
            _logger.LogInformation("VIP application already processed: {ApprovalId}", request.ApprovalId);
            return true;
        }

        var member = await _memberRepository.GetByIdAsync(approval.MemberId, cancellationToken);
        if (member == null)
        {
            _logger.LogWarning("Member not found for VIP application: {MemberId}", approval.MemberId);
            return false;
        }

        // Update the approval record
        approval.Approved = true;
        approval.ApprovedBy = request.ApprovedBy;
        approval.Reason = $"VIP_APPROVED: {request.Reason}";

        // Grant VIP status to the member
        member.VipStatus = true;
        member.UpdatedAt = DateTime.UtcNow;

        await _memberApprovalRepository.UpdateAsync(approval, cancellationToken);
        await _memberRepository.UpdateAsync(member, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("VIP application {ApprovalId} approved for member {MemberId} by {ApprovedBy}", 
            request.ApprovalId, member.Id, request.ApprovedBy);

        return true;
    }
}
