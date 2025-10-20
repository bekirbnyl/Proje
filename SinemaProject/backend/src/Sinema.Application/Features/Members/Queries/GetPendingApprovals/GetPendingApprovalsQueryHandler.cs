using MediatR;
using Sinema.Application.DTOs.Members;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Application.Features.Members.Queries.GetPendingApprovals;

/// <summary>
/// Handler for getting pending member approvals
/// </summary>
public class GetPendingApprovalsQueryHandler : IRequestHandler<GetPendingApprovalsQuery, IEnumerable<MemberApprovalDto>>
{
    private readonly IMemberRepository _memberRepository;

    public GetPendingApprovalsQueryHandler(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task<IEnumerable<MemberApprovalDto>> Handle(GetPendingApprovalsQuery request, CancellationToken cancellationToken)
    {
        // Get all members that are pending approval
        var pendingMembers = await _memberRepository.GetPendingApprovalMembersAsync(cancellationToken);

        var result = new List<MemberApprovalDto>();

        foreach (var member in pendingMembers)
        {
            // Check for VIP applications first
            var vipApplication = member.Approvals.FirstOrDefault(a => 
                a.Reason.StartsWith("VIP_APPLICATION:") && 
                !a.Approved && 
                a.ApprovedBy == null);
                
            if (vipApplication != null)
            {
                // This is a VIP application
                result.Add(new MemberApprovalDto
                {
                    Id = vipApplication.Id,
                    MemberId = member.Id,
                    Approved = false,
                    Reason = vipApplication.Reason,
                    ApprovedBy = vipApplication.ApprovedBy,
                    CreatedAt = vipApplication.CreatedAt,
                    Member = new MemberDto
                    {
                        Id = member.Id,
                        FullName = member.FullName,
                        Email = member.Email,
                        PhoneNumber = member.PhoneNumber,
                        UserId = member.UserId,
                        VipStatus = member.VipStatus,
                        CreatedAt = member.CreatedAt,
                        UpdatedAt = member.UpdatedAt,
                        IsApproved = member.IsApproved
                    }
                });
            }
            else
            {
                var latestApproval = member.LatestApproval;

                if (latestApproval != null && !latestApproval.Approved)
                {
                    result.Add(new MemberApprovalDto
                    {
                        Id = latestApproval.Id,
                        MemberId = member.Id,
                        Approved = false,
                        Reason = latestApproval.Reason,
                        ApprovedBy = latestApproval.ApprovedBy,
                        CreatedAt = latestApproval.CreatedAt,
                        Member = new MemberDto
                        {
                            Id = member.Id,
                            FullName = member.FullName,
                            Email = member.Email,
                            PhoneNumber = member.PhoneNumber,
                            UserId = member.UserId,
                            VipStatus = member.VipStatus,
                            CreatedAt = member.CreatedAt,
                            UpdatedAt = member.UpdatedAt,
                            IsApproved = false
                        }
                    });
                }
                else if (!member.Approvals.Any())
                {
                    // Member without any approval records (should create a pending one)
                    result.Add(new MemberApprovalDto
                    {
                        Id = Guid.Empty,
                        MemberId = member.Id,
                        Approved = false,
                        Reason = "Pending approval",
                        CreatedAt = member.CreatedAt,
                        Member = new MemberDto
                        {
                            Id = member.Id,
                            FullName = member.FullName,
                            Email = member.Email,
                            PhoneNumber = member.PhoneNumber,
                            UserId = member.UserId,
                            VipStatus = member.VipStatus,
                            CreatedAt = member.CreatedAt,
                            UpdatedAt = member.UpdatedAt,
                            IsApproved = false
                        }
                    });
                }
            }
        }

        return result.OrderByDescending(a => a.CreatedAt);
    }
}
