using MediatR;
using Microsoft.Extensions.Logging;
using Sinema.Domain.Entities;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Application.Features.Members.Commands.RejectMember;

/// <summary>
/// Handler for rejecting a member
/// </summary>
public class RejectMemberCommandHandler : IRequestHandler<RejectMemberCommand, bool>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RejectMemberCommandHandler> _logger;

    public RejectMemberCommandHandler(
        IMemberRepository memberRepository,
        IUnitOfWork unitOfWork,
        ILogger<RejectMemberCommandHandler> logger)
    {
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(RejectMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(request.MemberId, cancellationToken);

        if (member == null)
        {
            _logger.LogWarning("Member not found: {MemberId}", request.MemberId);
            return false;
        }

        // Create rejection record
        var rejection = MemberApproval.CreateRejection(
            request.MemberId,
            request.Reason,
            request.RejectedBy
        );

        // Add rejection to member (member is already tracked, no need to call UpdateAsync)
        member.Approvals.Add(rejection);
        
        // Just save changes - member is already tracked by EF Core
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Member {MemberId} rejected by {RejectedBy}", 
            request.MemberId, request.RejectedBy);

        return true;
    }
}
