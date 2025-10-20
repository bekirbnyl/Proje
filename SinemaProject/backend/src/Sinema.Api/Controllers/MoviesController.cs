using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinema.Api.Filters;
using Sinema.Application.DTOs.Movies;
using Sinema.Application.Features.Common.SoftDelete.Commands.RestoreEntity;
using Sinema.Application.Features.Common.SoftDelete.Commands.SoftDeleteEntity;
using Sinema.Application.Features.Movies.Commands.CreateMovie;
using Sinema.Application.Features.Movies.Commands.DeleteMovie;
using Sinema.Application.Features.Movies.Commands.UpdateMovie;
using Sinema.Application.Features.Movies.Queries.GetMovieById;
using Sinema.Application.Features.Movies.Queries.GetMovies;

namespace Sinema.Api.Controllers;

/// <summary>
/// Controller for movie management
/// </summary>
[ApiController]
[Route("api/v1/movies")]
[Produces("application/json")]
public class MoviesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MoviesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all movies with optional filtering by active status and deletion status
    /// </summary>
    /// <param name="active">Filter by active status (null = all, true = active only, false = inactive only)</param>
    /// <param name="includeDeleted">Include soft-deleted movies (Admin/Yonetim only)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of movies</returns>
    /// <response code="200">Returns the list of movies</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MovieDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MovieDto>>> GetMovies(
        [FromQuery] bool? active = null,
        [FromQuery] bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        // Check authorization for including deleted items
        if (includeDeleted && !User.IsInRole("Admin") && !User.IsInRole("Yonetim"))
        {
            return Forbid("Only Admin or Yonetim roles can view deleted movies");
        }

        var query = new GetMoviesQuery(active);
        var movies = await _mediator.Send(query, cancellationToken);
        return Ok(movies);
    }

    /// <summary>
    /// Get a specific movie by ID
    /// </summary>
    /// <param name="id">Movie ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Movie details</returns>
    /// <response code="200">Returns the movie</response>
    /// <response code="404">Movie not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MovieDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieDto>> GetMovie(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMovieByIdQuery(id);
        var movie = await _mediator.Send(query, cancellationToken);
        
        if (movie == null)
        {
            return NotFound($"Movie with ID '{id}' not found.");
        }

        return Ok(movie);
    }

    /// <summary>
    /// Create a new movie
    /// </summary>
    /// <param name="request">Movie creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created movie</returns>
    /// <response code="201">Movie created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="409">Movie with same title already exists</response>
    [HttpPost]
    [Authorize(Roles = "Admin,SinemaMuduru")]
    [ProducesResponseType(typeof(MovieDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MovieDto>> CreateMovie(
        [FromBody] CreateMovieRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateMovieCommand(
            request.Title,
            request.DurationMinutes,
            request.IsActive);

        try
        {
            var movie = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetMovie), new { id = movie.Id }, movie);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Update an existing movie
    /// </summary>
    /// <param name="id">Movie ID</param>
    /// <param name="request">Movie update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated movie</returns>
    /// <response code="200">Movie updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Movie not found</response>
    /// <response code="409">Movie with same title already exists</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,SinemaMuduru")]
    [ProducesResponseType(typeof(MovieDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MovieDto>> UpdateMovie(
        Guid id,
        [FromBody] UpdateMovieRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateMovieCommand(
            id,
            request.Title,
            request.DurationMinutes,
            request.IsActive);

        try
        {
            var movie = await _mediator.Send(command, cancellationToken);
            return Ok(movie);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Soft-delete a movie (requires deletion reason)
    /// </summary>
    /// <param name="id">Movie ID</param>
    /// <param name="reason">Reason for deletion (required)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Movie soft-deleted successfully</response>
    /// <response code="400">Movie has future screenings or invalid reason</response>
    /// <response code="404">Movie not found</response>
    [HttpDelete("{id:guid}")]
    [RequireDeleteReason]
    [Authorize(Roles = "Admin,SinemaMuduru")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMovie(
        Guid id,
        [FromQuery] string reason,
        CancellationToken cancellationToken = default)
    {
        var command = new SoftDeleteEntityCommand
        {
            EntityType = "Movie",
            EntityId = id,
            Reason = reason,
            UserId = GetCurrentUserId()
        };

        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            if (!result)
            {
                return NotFound($"Movie with ID '{id}' not found.");
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Restore a soft-deleted movie
    /// </summary>
    /// <param name="id">Movie ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>OK result</returns>
    /// <response code="200">Movie restored successfully</response>
    /// <response code="400">Movie cannot be restored due to dependencies</response>
    /// <response code="404">Movie not found or not deleted</response>
    [HttpPost("{id:guid}/restore")]
    [Authorize(Roles = "Admin,SinemaMuduru")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreMovie(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new RestoreEntityCommand
        {
            EntityType = "Movie",
            EntityId = id,
            UserId = GetCurrentUserId()
        };

        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            if (!result)
            {
                return NotFound($"Movie with ID '{id}' not found or not deleted.");
            }
            return Ok(new { message = "Movie restored successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets the current user's ID
    /// </summary>
    /// <returns>User ID as Guid or null</returns>
    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
