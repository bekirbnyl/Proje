using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinema.Api.Filters;
using Sinema.Application.DTOs.Screenings;
using Sinema.Application.Features.Screenings.Commands.CreateScreening;
using Sinema.Application.Features.Screenings.Commands.DeleteScreening;
using Sinema.Application.Features.Screenings.Commands.UpdateScreening;
using Sinema.Application.Features.Screenings.Queries.GetScreeningById;
using Sinema.Application.Features.Screenings.Queries.GetScreeningsByDate;

namespace Sinema.Api.Controllers;

/// <summary>
/// Controller for screening management
/// </summary>
[ApiController]
[Route("api/v1/screenings")]
[Produces("application/json")]
public class ScreeningsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ScreeningsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get screenings filtered by date, hall, and/or movie
    /// </summary>
    /// <param name="date">Filter by date (YYYY-MM-DD format)</param>
    /// <param name="hallId">Filter by hall ID</param>
    /// <param name="movieId">Filter by movie ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of screenings</returns>
    /// <response code="200">Returns the list of screenings</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ScreeningListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ScreeningListItemDto>>> GetScreenings(
        [FromQuery] DateTime? date = null,
        [FromQuery] Guid? hallId = null,
        [FromQuery] Guid? movieId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetScreeningsByDateQuery(date, hallId, movieId);
        var screenings = await _mediator.Send(query, cancellationToken);
        return Ok(screenings);
    }

    /// <summary>
    /// Get a specific screening by ID
    /// </summary>
    /// <param name="id">Screening ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Screening details</returns>
    /// <response code="200">Returns the screening</response>
    /// <response code="404">Screening not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ScreeningDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ScreeningDto>> GetScreening(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetScreeningByIdQuery(id);
        var screening = await _mediator.Send(query, cancellationToken);
        
        if (screening == null)
        {
            return NotFound($"Screening with ID '{id}' not found.");
        }

        return Ok(screening);
    }

    /// <summary>
    /// Create a new screening
    /// </summary>
    /// <param name="request">Screening creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created screening</returns>
    /// <response code="201">Screening created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="409">Screening conflicts with existing screening</response>
    [HttpPost]
    [Authorize(Roles = "Admin,SinemaMuduru")]
    [ProducesResponseType(typeof(ScreeningDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ScreeningDto>> CreateScreening(
        [FromBody] CreateScreeningRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateScreeningCommand(
            request.MovieId,
            request.HallId,
            request.StartAt,
            request.DurationMinutes,
            request.SeatLayoutId);

        try
        {
            var screening = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetScreening), new { id = screening.Id }, screening);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("conflicts") || ex.Message.Contains("overlap"))
        {
            return Conflict(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update an existing screening
    /// </summary>
    /// <param name="id">Screening ID</param>
    /// <param name="request">Screening update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated screening</returns>
    /// <response code="200">Screening updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Screening not found</response>
    /// <response code="409">Screening conflicts with existing screening</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,SinemaMuduru")]
    [ProducesResponseType(typeof(ScreeningDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ScreeningDto>> UpdateScreening(
        Guid id,
        [FromBody] UpdateScreeningRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateScreeningCommand(
            id,
            request.MovieId,
            request.HallId,
            request.StartAt,
            request.DurationMinutes,
            request.SeatLayoutId);

        try
        {
            var screening = await _mediator.Send(command, cancellationToken);
            return Ok(screening);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("conflicts") || ex.Message.Contains("overlap"))
        {
            return Conflict(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Delete a screening
    /// </summary>
    /// <param name="id">Screening ID</param>
    /// <param name="reason">Reason for deletion (required)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Screening deleted successfully</response>
    /// <response code="400">Cannot delete past screenings or invalid reason</response>
    /// <response code="404">Screening not found</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,SinemaMuduru")]
    [RequireDeleteReason]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteScreening(
        Guid id,
        [FromQuery] string reason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return BadRequest("Deletion reason is required.");
        }

        var command = new DeleteScreeningCommand(id, reason);

        try
        {
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}