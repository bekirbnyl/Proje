using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinema.Application.DTOs.Members;
using Sinema.Application.Features.Members.Commands.ApproveMember;
using Sinema.Application.Features.Members.Commands.GrantVipStatus;
using Sinema.Application.Features.Members.Commands.RejectMember;
using Sinema.Application.Features.Members.Commands.RevokeVipStatus;
using Sinema.Application.Features.Members.Commands.ApplyForVip;
using Sinema.Application.Features.Members.Commands.ApproveVipApplication;
using Sinema.Application.Features.Members.Queries.GetPendingApprovals;

namespace Sinema.Api.Controllers;

/// <summary>
/// Controller for member management
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin")]
public class MembersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<MembersController> _logger;

    public MembersController(IMediator mediator, ILogger<MembersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all pending member approvals
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of pending member approvals</returns>
    [HttpGet("pending-approvals")]
    [ProducesResponseType(typeof(IEnumerable<MemberApprovalDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MemberApprovalDto>>> GetPendingApprovals(CancellationToken cancellationToken)
    {
        var query = new GetPendingApprovalsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Approve a member
    /// </summary>
    /// <param name="memberId">Member ID</param>
    /// <param name="request">Approval request with reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("{memberId:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveMember(
        Guid memberId,
        [FromBody] ApproveRejectRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Debug: Check authentication and authorization
            _logger.LogInformation("User authenticated: {IsAuthenticated}", User.Identity?.IsAuthenticated);
            _logger.LogInformation("User roles: {Roles}", string.Join(", ", User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value)));
            
            if (!User.IsInRole("Admin"))
            {
                _logger.LogWarning("User does not have Admin role. Available roles: {Roles}", 
                    string.Join(", ", User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value)));
                return Forbid("Admin role required");
            }

            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                _logger.LogWarning("Approval attempted without valid user ID");
                return BadRequest("Unable to identify current user.");
            }

            _logger.LogInformation("Attempting to approve member {MemberId} by user {UserId}", memberId, currentUserId);

            var command = new ApproveMemberCommand
            {
                MemberId = memberId,
                Reason = request.Reason,
                ApprovedBy = currentUserId
            };

            var result = await _mediator.Send(command, cancellationToken);
            
            if (!result)
            {
                _logger.LogWarning("Failed to approve member {MemberId} - member not found or already approved", memberId);
                return NotFound($"Member with ID '{memberId}' not found or already approved.");
            }

            _logger.LogInformation("Successfully approved member {MemberId}", memberId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while approving member {MemberId}: {Message}", memberId, ex.Message);
            return StatusCode(500, new { 
                error = "Internal server error", 
                message = ex.Message,
                type = ex.GetType().Name 
            });
        }
    }

    /// <summary>
    /// Reject a member
    /// </summary>
    /// <param name="memberId">Member ID</param>
    /// <param name="request">Rejection request with reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("{memberId:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectMember(
        Guid memberId,
        [FromBody] ApproveRejectRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RejectMemberCommand
        {
            MemberId = memberId,
            Reason = request.Reason,
            RejectedBy = GetCurrentUserId()
        };

        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound($"Member with ID '{memberId}' not found.");
        }

        return NoContent();
    }

    /// <summary>
    /// Grant VIP status to a member
    /// </summary>
    /// <param name="memberId">Member ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("{memberId:guid}/grant-vip")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GrantVipStatus(
        Guid memberId,
        CancellationToken cancellationToken)
    {
        var command = new GrantVipStatusCommand
        {
            MemberId = memberId,
            GrantedBy = GetCurrentUserId()
        };

        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound($"Member with ID '{memberId}' not found.");
        }

        return NoContent();
    }

    /// <summary>
    /// Revoke VIP status from a member
    /// </summary>
    /// <param name="memberId">Member ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("{memberId:guid}/revoke-vip")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeVipStatus(
        Guid memberId,
        CancellationToken cancellationToken)
    {
        var command = new RevokeVipStatusCommand
        {
            MemberId = memberId,
            RevokedBy = GetCurrentUserId()
        };

        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound($"Member with ID '{memberId}' not found.");
        }

        return NoContent();
    }

    /// <summary>
    /// Gets the current user's ID
    /// </summary>
    /// <returns>User ID as Guid or null</returns>
    private Guid? GetCurrentUserId()
    {
        // Debug: Log all available claims
        var allClaims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToArray();
        _logger.LogInformation("Available claims: {Claims}", string.Join(", ", allClaims));
        
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            // Try alternative claim types
            userIdClaim = User.FindFirst("sub")?.Value;
            _logger.LogWarning("NameIdentifier not found, using 'sub' claim: {SubClaim}", userIdClaim);
        }
        
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogInformation("Successfully parsed user ID: {UserId}", userId);
            return userId;
        }
        
        _logger.LogWarning("Failed to parse user ID from claim: {UserIdClaim}", userIdClaim);
        return null;
    }

    /// <summary>
    /// Apply for VIP status
    /// </summary>
    /// <param name="request">VIP application request</param>
    /// <returns>Success status</returns>
    [HttpPost("vip-application")]
    [Authorize] // Any authenticated user can apply
    public async Task<IActionResult> ApplyForVip([FromBody] VipApplicationRequest request)
    {
        // Get current user's member ID from claims
        var memberIdClaim = User.FindFirst("member_id")?.Value;
        if (string.IsNullOrEmpty(memberIdClaim) || !Guid.TryParse(memberIdClaim, out var memberId))
        {
            return BadRequest("User must have a member record to apply for VIP status.");
        }

        // Get current user ID
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return BadRequest("Unable to identify current user.");
        }

        var command = new ApplyForVipCommand
        {
            MemberId = memberId,
            Reason = request.Reason,
            UserId = userIdClaim
        };

        var result = await _mediator.Send(command);

        if (result)
        {
            return Ok(new { Message = "VIP application submitted successfully." });
        }

        return BadRequest("Failed to submit VIP application.");
    }

    /// <summary>
    /// Approve a VIP application
    /// </summary>
    /// <param name="approvalId">Approval ID</param>
    /// <param name="request">Approval request with reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("approvals/{approvalId:guid}/approve-vip")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveVipApplication(
        Guid approvalId,
        [FromBody] ApproveRejectRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ApproveVipApplicationCommand
        {
            ApprovalId = approvalId,
            Reason = request.Reason,
            ApprovedBy = GetCurrentUserId()
        };

        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound($"VIP application with ID '{approvalId}' not found.");
        }

        return NoContent();
    }
}

/// <summary>
/// Request model for approve/reject operations
/// </summary>
public class ApproveRejectRequest
{
    /// <summary>
    /// Reason for the approval/rejection
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Request model for VIP application
/// </summary>
public class VipApplicationRequest
{
    /// <summary>
    /// Reason for VIP application
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
