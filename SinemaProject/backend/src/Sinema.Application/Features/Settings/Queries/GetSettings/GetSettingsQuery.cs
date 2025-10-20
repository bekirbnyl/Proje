using MediatR;
using Sinema.Application.DTOs.Settings;

namespace Sinema.Application.Features.Settings.Queries.GetSettings;

/// <summary>
/// Query to get settings with whitelist filtering
/// </summary>
public record GetSettingsQuery : IRequest<Dictionary<string, SettingDto>>
{
    /// <summary>
    /// Optional keys to filter. If null or empty, returns all whitelisted settings
    /// </summary>
    public string[]? Keys { get; init; }
}
