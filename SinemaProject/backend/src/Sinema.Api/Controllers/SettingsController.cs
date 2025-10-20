using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinema.Application.DTOs.Settings;
using Sinema.Application.Features.Settings.Commands.PatchSetting;
using Sinema.Application.Features.Settings.Commands.UpdateSettings;
using Sinema.Application.Features.Settings.Queries.GetSettings;
using System.ComponentModel.DataAnnotations;

namespace Sinema.Api.Controllers;

/// <summary>
/// Controller for managing application settings
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets application settings (whitelist filtered)
    /// </summary>
    /// <param name="keys">Optional specific keys to retrieve</param>
    /// <returns>Dictionary of setting key-value pairs</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Yonetim")]
    [ProducesResponseType(typeof(Dictionary<string, SettingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Dictionary<string, SettingDto>>> GetSettings([FromQuery] string[]? keys = null)
    {
        var query = new GetSettingsQuery { Keys = keys };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Updates multiple settings
    /// </summary>
    /// <param name="request">Settings update request</param>
    /// <returns>Updated settings with new row versions</returns>
    [HttpPut]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Dictionary<string, SettingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Dictionary<string, SettingDto>>> UpdateSettings([FromBody] UpdateSettingsRequest request)
    {
        var command = new UpdateSettingsCommand
        {
            Settings = request.Items.ToDictionary(i => i.Key, i => i.Value),
            UserId = GetCurrentUserId()
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Updates a single setting with concurrency control
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <param name="request">Patch request with value and optional row version</param>
    /// <returns>Updated setting</returns>
    [HttpPatch("{key}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SettingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SettingDto>> PatchSetting(
        [FromRoute] [Required] string key,
        [FromBody] PatchSettingRequest request)
    {
        // Parse row version from base64 if provided
        byte[]? rowVersion = null;
        if (!string.IsNullOrEmpty(request.RowVersion))
        {
            try
            {
                rowVersion = Convert.FromBase64String(request.RowVersion);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid row version format. Expected base64 string.");
            }
        }

        // Check If-Match header as alternative to body row version
        if (rowVersion == null && Request.Headers.ContainsKey("If-Match"))
        {
            var ifMatch = Request.Headers["If-Match"].FirstOrDefault();
            if (!string.IsNullOrEmpty(ifMatch))
            {
                try
                {
                    rowVersion = Convert.FromBase64String(ifMatch.Trim('"'));
                }
                catch (FormatException)
                {
                    return BadRequest("Invalid If-Match header format. Expected base64 string.");
                }
            }
        }

        var command = new PatchSettingCommand
        {
            Key = key,
            Value = request.Value,
            RowVersion = rowVersion,
            UserId = GetCurrentUserId()
        };

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        {
            return Conflict(new { message = "Setting has been modified by another user. Please refresh and try again." });
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
