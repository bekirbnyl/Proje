using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinema.Application.DTOs.Reservations;
using Sinema.Application.Features.Reservations.Commands.CreateReservation;
using Sinema.Infrastructure.Auth;
using System.Security.Claims;

namespace Sinema.Api.Controllers;

/// <summary>
/// Controller for reservation operations
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class ReservationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReservationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Creates reservations for multiple seats (requires active holds by the same client)
    /// </summary>
    /// <param name="request">Reservation creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of created reservations</returns>
    /// <response code="201">Reservations created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="409">Seats not held by the requesting client or already reserved</response>
    /// <response code="422">Business rule violation (e.g., T-60 rule)</response>
    [HttpPost]
    [Authorize(Policy = "ApprovedMemberOnly")]
    [ProducesResponseType(typeof(List<ReservationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<List<ReservationDto>>> CreateReservations(
        [FromBody] CreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            var command = new CreateReservationCommand
            {
                ScreeningId = request.ScreeningId,
                SeatIds = request.SeatIds,
                ClientToken = request.ClientToken,
                MemberId = userId
            };

            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(CreateReservations), result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("within 60 minutes"))
        {
            return UnprocessableEntity(new { message = ex.Message, code = "T_MINUS_60_VIOLATION" });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("must be held"))
        {
            return Conflict(new { message = ex.Message, code = "SEATS_NOT_HELD" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Confirms a reservation (payment processing would happen here)
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Confirmed reservation</returns>
    /// <response code="200">Reservation confirmed successfully</response>
    /// <response code="404">Reservation not found</response>
    /// <response code="422">Cannot confirm (e.g., T-30 rule violation or already confirmed)</response>
    [HttpPost("{id}/confirm")]
    [Authorize(Policy = "ApprovedMemberOnly")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ReservationDto>> ConfirmReservation(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        // TODO: Implement ConfirmReservationCommand and handler
        // This would handle T-30 rule validation and payment processing
        return StatusCode(501, new { message = "Not implemented yet" });
    }

    /// <summary>
    /// Cancels a reservation
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <param name="reason">Cancellation reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Reservation cancelled successfully</response>
    /// <response code="404">Reservation not found</response>
    /// <response code="422">Cannot cancel (e.g., already confirmed)</response>
    [HttpPost("{id}/cancel")]
    [Authorize(Policy = "ApprovedMemberOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CancelReservation(
        [FromRoute] Guid id,
        [FromQuery] string? reason,
        CancellationToken cancellationToken)
    {
        // TODO: Implement CancelReservationCommand and handler
        return StatusCode(501, new { message = "Not implemented yet" });
    }

    /// <summary>
    /// Gets a reservation by ID
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Reservation details</returns>
    /// <response code="200">Reservation found</response>
    /// <response code="404">Reservation not found</response>
    [HttpGet("{id}")]
    [Authorize(Policy = "ApprovedMemberOnly")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDto>> GetReservation(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        // TODO: Implement GetReservationQuery and handler
        return StatusCode(501, new { message = "Not implemented yet" });
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
