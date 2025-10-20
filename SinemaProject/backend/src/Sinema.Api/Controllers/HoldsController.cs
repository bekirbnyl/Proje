using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinema.Application.DTOs.Reservations.Holds;
using Sinema.Application.Features.Reservations.Holds.Commands.CreateSeatHolds;
using Sinema.Application.Features.Reservations.Holds.Commands.ExtendSeatHold;
using Sinema.Application.Features.Reservations.Holds.Commands.ReleaseSeatHold;
using Sinema.Infrastructure.Auth;
using System.Security.Claims;

namespace Sinema.Api.Controllers;

/// <summary>
/// Controller for seat hold operations
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class HoldsController : ControllerBase
{
    private readonly IMediator _mediator;

    public HoldsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Creates holds for multiple seats in a screening
    /// </summary>
    /// <param name="request">Hold creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of created holds</returns>
    /// <response code="201">Holds created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="409">One or more seats are already held, reserved, or sold</response>
    /// <response code="422">Business rule violation (e.g., T-60 rule)</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(List<SeatHoldDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<List<SeatHoldDto>>> CreateHolds(
        [FromBody] CreateSeatHoldsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            var command = new CreateSeatHoldsCommand
            {
                ScreeningId = request.ScreeningId,
                SeatIds = request.SeatIds,
                ClientToken = request.ClientToken,
                UserId = userId,
                TtlSeconds = request.TtlSeconds
            };

            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(CreateHolds), result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("within 60 minutes"))
        {
            return UnprocessableEntity(new { message = ex.Message, code = "T_MINUS_60_VIOLATION" });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already held") || ex.Message.Contains("already reserved") || ex.Message.Contains("already sold"))
        {
            return Conflict(new { message = ex.Message, code = "SEATS_UNAVAILABLE" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Extends the expiration time of a hold (heartbeat mechanism)
    /// </summary>
    /// <param name="holdId">Hold ID to extend</param>
    /// <param name="request">Extension request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated hold with new expiration time</returns>
    /// <response code="200">Hold extended successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="403">Not authorized to extend this hold</response>
    /// <response code="404">Hold not found</response>
    /// <response code="410">Hold has expired</response>
    [HttpPost("{holdId}/extend")]
    [Authorize]
    [ProducesResponseType(typeof(SeatHoldDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status410Gone)]
    public async Task<ActionResult<SeatHoldDto>> ExtendHold(
        [FromRoute] Guid holdId,
        [FromBody] ExtendSeatHoldRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            var command = new ExtendSeatHoldCommand
            {
                HoldId = holdId,
                ClientToken = request.ClientToken,
                UserId = userId
            };

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("expired"))
        {
            return StatusCode(410, new { message = ex.Message, code = "HOLD_EXPIRED" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Releases a hold manually
    /// </summary>
    /// <param name="holdId">Hold ID to release</param>
    /// <param name="clientToken">Client token for verification</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Hold released successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="403">Not authorized to release this hold</response>
    /// <response code="404">Hold not found</response>
    [HttpDelete("{holdId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReleaseHold(
        [FromRoute] Guid holdId,
        [FromQuery] string clientToken,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(clientToken))
            {
                return BadRequest(new { message = "Client token is required" });
            }

            var userId = GetCurrentUserId();
            
            var command = new ReleaseSeatHoldCommand
            {
                HoldId = holdId,
                ClientToken = clientToken,
                UserId = userId
            };

            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets the current user ID from the JWT token
    /// </summary>
    /// <returns>User ID if authenticated, null otherwise</returns>
    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return null;
        }

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
