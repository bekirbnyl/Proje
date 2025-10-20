using MediatR;
using Sinema.Application.DTOs.Settings;

namespace Sinema.Application.Features.Settings.Commands.UpdateSettings;

/// <summary>
/// Command to update multiple settings
/// </summary>
public record UpdateSettingsCommand : IRequest<Dictionary<string, SettingDto>>
{
    /// <summary>
    /// Settings to update
    /// </summary>
    public Dictionary<string, string> Settings { get; init; } = new();

    /// <summary>
    /// ID of the user making the changes
    /// </summary>
    public Guid? UserId { get; init; }
}
