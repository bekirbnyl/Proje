using MediatR;
using Microsoft.Extensions.Logging;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Application.Features.Members.Commands.GrantVipStatus;

/// <summary>
/// Handler for granting VIP status to a member
/// </summary>
public class GrantVipStatusCommandHandler : IRequestHandler<GrantVipStatusCommand, bool>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GrantVipStatusCommandHandler> _logger;

    public GrantVipStatusCommandHandler(
        IMemberRepository memberRepository,
        IUnitOfWork unitOfWork,
        ILogger<GrantVipStatusCommandHandler> logger)
    {
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(GrantVipStatusCommand request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(request.MemberId, cancellationToken);

        if (member == null)
        {
            _logger.LogWarning("Member not found: {MemberId}", request.MemberId);
            return false;
        }

        if (member.VipStatus)
        {
            _logger.LogInformation("Member already has VIP status: {MemberId}", request.MemberId);
            return true;
        }

        member.GrantVipStatus(request.VipStartDate, request.VipEndDate, request.VipPaymentId);

        // Member is already tracked by EF Core, just save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("VIP status granted to member {MemberId} by {GrantedBy}", 
            request.MemberId, request.GrantedBy);

        return true;
    }
}
