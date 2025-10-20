using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sinema.Application.DTOs.Seating;
using Sinema.Application.Features.Halls.Commands.ActivateSeatLayoutVersion;
using Sinema.Application.Features.Halls.Commands.AddSeatLayoutVersion;
using Sinema.Application.Features.Halls.Queries.GetActiveLayoutByHall;
using Sinema.Application.Features.Halls.Queries.GetSeatLayoutsByHall;
using Sinema.Infrastructure.Persistence;

namespace Sinema.Api.Controllers;

/// <summary>
/// Hall management and seat layout versioning endpoints
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class HallsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<HallsController> _logger;
    private readonly SinemaDbContext _context;

    public HallsController(IMediator mediator, ILogger<HallsController> logger, SinemaDbContext context)
    {
        _mediator = mediator;
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Get all halls
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all halls</returns>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAllHalls(CancellationToken cancellationToken = default)
    {
        try
        {
            // Simple query to get all halls with their IDs and names
            var halls = await _context.Halls
                .Select(h => new { h.Id, h.Name, h.CinemaId })
                .ToListAsync(cancellationToken);

            return Ok(halls);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all halls");
            return Problem("An error occurred while retrieving halls.", statusCode: 500);
        }
    }

    /// <summary>
    /// Get all seat layout versions for a hall
    /// </summary>
    /// <param name="hallId">Hall identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of seat layout versions ordered by version (latest first)</returns>
    [HttpGet("{hallId:guid}/layouts")]
    [ProducesResponseType(typeof(IEnumerable<SeatLayoutDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetSeatLayoutsByHall(
        [FromRoute] Guid hallId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetSeatLayoutsByHallQuery { HallId = hallId };
            var layouts = await _mediator.Send(query, cancellationToken);

            return Ok(layouts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting seat layouts for hall {HallId}", hallId);
            return Problem("An error occurred while retrieving seat layouts.", statusCode: 500);
        }
    }

    /// <summary>
    /// Get the active seat layout for a hall
    /// </summary>
    /// <param name="hallId">Hall identifier</param>
    /// <param name="version">Version parameter (should be 'active')</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Active seat layout with seats</returns>
    [HttpGet("{hallId:guid}/layout")]
    [ProducesResponseType(typeof(SeatLayoutDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetActiveLayoutByHall(
        [FromRoute] Guid hallId,
        [FromQuery] string? version = null,
        CancellationToken cancellationToken = default)
    {
        // For now, we only support 'active' version query
        if (version != null && version != "active")
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Version Parameter",
                Detail = "Currently only 'active' version is supported.",
                Status = 400
            });
        }

        try
        {
            var query = new GetActiveLayoutByHallQuery { HallId = hallId };
            var layout = await _mediator.Send(query, cancellationToken);

            if (layout == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Active Layout Not Found",
                    Detail = $"No active seat layout found for hall {hallId}.",
                    Status = 404
                });
            }

            return Ok(layout);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active layout for hall {HallId}", hallId);
            return Problem("An error occurred while retrieving the active layout.", statusCode: 500);
        }
    }

    /// <summary>
    /// Add a new seat layout version to a hall
    /// </summary>
    /// <param name="hallId">Hall identifier</param>
    /// <param name="request">Seat layout creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created seat layout version</returns>
    [HttpPost("{hallId:guid}/layouts")]
    [Authorize(Roles = "Admin,SinemaMuduru,GiseAmiri")]
    [ProducesResponseType(typeof(SeatLayoutDto), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 403)]
    public async Task<IActionResult> AddSeatLayoutVersion(
        [FromRoute] Guid hallId,
        [FromBody] AddSeatLayoutVersionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new AddSeatLayoutVersionCommand
            {
                HallId = hallId,
                Seats = request.Seats
            };

            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("New seat layout version {Version} created for hall {HallId}", 
                result.Version, hallId);

            return CreatedAtAction(
                nameof(GetSeatLayoutsByHall),
                new { hallId },
                result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = 400
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating seat layout version for hall {HallId}", hallId);
            return Problem("An error occurred while creating the seat layout version.", statusCode: 500);
        }
    }

    /// <summary>
    /// Activate a specific seat layout version for a hall
    /// </summary>
    /// <param name="hallId">Hall identifier</param>
    /// <param name="layoutId">Layout identifier to activate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("{hallId:guid}/layouts/{layoutId:guid}/activate")]
    [Authorize(Roles = "Admin,SinemaMuduru,GiseAmiri")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 403)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> ActivateSeatLayoutVersion(
        [FromRoute] Guid hallId,
        [FromRoute] Guid layoutId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new ActivateSeatLayoutVersionCommand
            {
                HallId = hallId,
                LayoutId = layoutId
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result)
            {
                _logger.LogInformation("Seat layout {LayoutId} activated for hall {HallId}", 
                    layoutId, hallId);
                return NoContent();
            }

            return BadRequest(new ProblemDetails
            {
                Title = "Activation Failed",
                Detail = "Failed to activate the seat layout version.",
                Status = 400
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = 400
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating seat layout {LayoutId} for hall {HallId}", 
                layoutId, hallId);
            return Problem("An error occurred while activating the seat layout version.", statusCode: 500);
        }
    }
}
