using MediatR;
using Microsoft.Extensions.Logging;
using Sinema.Domain.Entities;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Application.Features.Members.Commands.ApproveMember;

/// <summary>
/// Handler for approving a member
/// </summary>
public class ApproveMemberCommandHandler : IRequestHandler<ApproveMemberCommand, bool>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IMemberApprovalRepository _memberApprovalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ApproveMemberCommandHandler> _logger;

    public ApproveMemberCommandHandler(
        IMemberRepository memberRepository,
        IMemberApprovalRepository memberApprovalRepository,
        IUnitOfWork unitOfWork,
        ILogger<ApproveMemberCommandHandler> logger)
    {
        _memberRepository = memberRepository;
        _memberApprovalRepository = memberApprovalRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(ApproveMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing approval for member {MemberId}", request.MemberId);

            // Don't use Include for Approvals to avoid tracking issues
            var member = await _memberRepository.GetByIdWithoutRelationsAsync(request.MemberId, cancellationToken);

            if (member == null)
            {
                _logger.LogWarning("Member not found: {MemberId}", request.MemberId);
                return false;
            }

            // Check if already approved separately
            var approvals = await _memberApprovalRepository.GetByMemberIdAsync(request.MemberId, cancellationToken);
            var hasApproval = approvals.Any(a => a.Approved);

            if (hasApproval)
            {
                _logger.LogInformation("Member already approved: {MemberId}", request.MemberId);
                return true;
            }

            if (request.ApprovedBy == null)
            {
                _logger.LogWarning("Approval attempted without ApprovedBy for member {MemberId}", request.MemberId);
                return false;
            }

            // Create approval record
            var approval = MemberApproval.CreateApproval(
                request.MemberId,
                request.Reason,
                request.ApprovedBy
            );

            // Add approval directly through repository
            await _memberApprovalRepository.AddAsync(approval, cancellationToken);
            
            _logger.LogInformation("Created approval record {ApprovalId} for member {MemberId}", 
                approval.Id, request.MemberId);

            // Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Member {MemberId} approved by {ApprovedBy} with approval {ApprovalId}", 
                request.MemberId, request.ApprovedBy, approval.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in ApproveMemberCommandHandler for member {MemberId}: {Message}", 
                request.MemberId, ex.Message);
            throw; // Re-throw to let controller handle it
        }
    }
}
