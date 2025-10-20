using MediatR;
using Microsoft.Extensions.Logging;
using Sinema.Domain.Interfaces.Repositories;
using Sinema.Domain.Entities;

namespace Sinema.Application.Features.Members.Commands.ApplyForVip;

/// <summary>
/// Handler for VIP application command
/// </summary>
public class ApplyForVipCommandHandler : IRequestHandler<ApplyForVipCommand, bool>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ApplyForVipCommandHandler> _logger;

    public ApplyForVipCommandHandler(
        IMemberRepository memberRepository,
        IUnitOfWork unitOfWork,
        ILogger<ApplyForVipCommandHandler> logger)
    {
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(ApplyForVipCommand request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(request.MemberId, cancellationToken);

        if (member == null)
        {
            _logger.LogWarning("Member not found for VIP application: {MemberId}", request.MemberId);
            return false;
        }

        if (member.VipStatus)
        {
            _logger.LogInformation("Member already has VIP status: {MemberId}", request.MemberId);
            return true;
        }

        // Check if there's already a pending VIP application
        var existingApplication = member.Approvals.FirstOrDefault(a => 
            a.Reason.StartsWith("VIP_APPLICATION:") && 
            !a.Approved && 
            a.ApprovedBy == null);

        if (existingApplication != null)
        {
            _logger.LogInformation("Member already has a pending VIP application: {MemberId}", request.MemberId);
            return true;
        }

        // Create a new VIP application approval record
        var vipApplication = new MemberApproval
        {
            Id = Guid.NewGuid(),
            MemberId = member.Id,
            Approved = false, // Pending approval
            Reason = $"VIP_APPLICATION: {request.Reason}",
            CreatedAt = DateTime.UtcNow
        };

        member.Approvals.Add(vipApplication);
        member.UpdatedAt = DateTime.UtcNow;

        await _memberRepository.UpdateAsync(member, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("VIP application created for member {MemberId} by user {UserId}", 
            request.MemberId, request.UserId);

        return true;
    }
}
