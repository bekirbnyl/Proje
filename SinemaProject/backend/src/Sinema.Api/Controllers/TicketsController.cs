using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinema.Application.DTOs.Tickets;
using Sinema.Application.Features.Tickets.Commands.SellTicket;
using Sinema.Application.Features.Tickets.Queries.GetTicketById;
using Sinema.Domain.Enums;

namespace Sinema.Api.Controllers;

/// <summary>
/// Controller for ticket sales and management operations
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Tickets")]
public class TicketsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TicketsController> _logger;

    public TicketsController(IMediator mediator, ILogger<TicketsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Sells tickets either from an existing reservation or directly (box office)
    /// </summary>
    /// <param name="request">Ticket sale request</param>
    /// <param name="idempotencyKey">Optional idempotency key to prevent duplicate transactions</param>
    /// <returns>Ticket sale response with payment status and ticket details</returns>
    /// <response code="200">Tickets sold successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="402">Payment failed</response>
    /// <response code="403">User not authorized for this operation</response>
    /// <response code="409">Conflict - seats already sold or held by another user</response>
    /// <response code="422">Business rule violation (e.g., reservation expired, invalid state)</response>
    [HttpPost]
    [ProducesResponseType(typeof(SellTicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<SellTicketResponse>> SellTickets(
        [FromBody] SellTicketRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey = null)
    {
        try
        {
            // Set idempotency key if provided
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                request.IdempotencyKey = idempotencyKey;
            }

            // Determine authorization requirements based on sale type and channel
            if (request.ReservationId.HasValue || request.Channel == TicketChannel.Online)
            {
                // Web/Online sales require approved member authentication
                if (!User.Identity?.IsAuthenticated == true)
                {
                    return Unauthorized("Authentication required for online sales");
                }

                if (!User.IsInRole("ApprovedMember") && !User.IsInRole("VipMember"))
                {
                    return Forbid("Only approved members can purchase tickets online");
                }
            }
            else if (request.Channel == TicketChannel.BoxOffice)
            {
                // Box office sales require staff authentication
                if (!User.Identity?.IsAuthenticated == true)
                {
                    return Unauthorized("Authentication required for box office sales");
                }

                if (!User.IsInRole("GiseGorevlisi") && !User.IsInRole("GiseAmiri") && 
                    !User.IsInRole("SinemaMuduru") && !User.IsInRole("Admin"))
                {
                    return Forbid("Insufficient permissions for box office sales");
                }
            }

            var command = new SellTicketCommand(request);
            var response = await _mediator.Send(command);

            // Map payment status to appropriate HTTP status
            return response.PaymentStatus switch
            {
                PaymentStatus.Succeeded => Ok(response),
                PaymentStatus.Failed => StatusCode(StatusCodes.Status402PaymentRequired, response),
                _ => BadRequest(response)
            };
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request parameters for ticket sale");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already sold") || ex.Message.Contains("held by another"))
        {
            _logger.LogWarning(ex, "Seat conflict during ticket sale");
            return Conflict(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation during ticket sale");
            return UnprocessableEntity(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during ticket sale");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An unexpected error occurred while processing the ticket sale" });
        }
    }

    /// <summary>
    /// Gets ticket details by ID
    /// </summary>
    /// <param name="id">Ticket ID</param>
    /// <returns>Ticket details</returns>
    /// <response code="200">Ticket found</response>
    /// <response code="404">Ticket not found</response>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(GetTicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetTicketResponse>> GetTicket(Guid id)
    {
        try
        {
            var query = new GetTicketByIdQuery(id);
            var response = await _mediator.Send(query);

            if (response == null)
            {
                return NotFound(new { error = $"Ticket with ID '{id}' not found" });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ticket {TicketId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while retrieving the ticket" });
        }
    }

    /// <summary>
    /// Voids a ticket (box office/admin only)
    /// </summary>
    /// <param name="id">Ticket ID</param>
    /// <param name="reason">Reason for voiding the ticket</param>
    /// <returns>Void operation result</returns>
    /// <response code="200">Ticket voided successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="403">Insufficient permissions</response>
    /// <response code="404">Ticket not found</response>
    [HttpPost("{id:guid}/void")]
    [Authorize(Roles = "GiseGorevlisi,GiseAmiri,SinemaMuduru,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> VoidTicket(Guid id, [FromQuery] string reason)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return BadRequest(new { error = "Reason is required for voiding a ticket" });
            }

            // Note: Void ticket functionality would be implemented here
            // For now, return a placeholder response
            _logger.LogInformation("Void ticket request for {TicketId} with reason: {Reason}", id, reason);
            
            return Ok(new { message = "Ticket void functionality not yet implemented", ticketId = id, reason });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voiding ticket {TicketId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while voiding the ticket" });
        }
    }
}
