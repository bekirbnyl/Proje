using MediatR;
using Microsoft.Extensions.Logging;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Application.Features.Members.Commands.RevokeVipStatus;

/// <summary>
/// Handler for revoking VIP status from a member
/// </summary>
public class RevokeVipStatusCommandHandler : IRequestHandler<RevokeVipStatusCommand, bool>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RevokeVipStatusCommandHandler> _logger;

    public RevokeVipStatusCommandHandler(
        IMemberRepository memberRepository,
        IUnitOfWork unitOfWork,
        ILogger<RevokeVipStatusCommandHandler> logger)
    {
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(RevokeVipStatusCommand request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(request.MemberId, cancellationToken);

        if (member == null)
        {
            _logger.LogWarning("Member not found: {MemberId}", request.MemberId);
            return false;
        }

        if (!member.VipStatus)
        {
            _logger.LogInformation("Member does not have VIP status: {MemberId}", request.MemberId);
            return true;
        }

        member.VipStatus = false;
        member.UpdatedAt = DateTime.UtcNow;

        // Member is already tracked by EF Core, just save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("VIP status revoked from member {MemberId} by {RevokedBy}", 
            request.MemberId, request.RevokedBy);

        return true;
    }
}
