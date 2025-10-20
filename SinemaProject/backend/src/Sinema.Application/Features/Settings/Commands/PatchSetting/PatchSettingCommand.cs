using MediatR;
using Sinema.Application.DTOs.Settings;

namespace Sinema.Application.Features.Settings.Commands.PatchSetting;

/// <summary>
/// Command to patch a single setting with concurrency control
/// </summary>
public record PatchSettingCommand : IRequest<SettingDto>
{
    /// <summary>
    /// Setting key to update
    /// </summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>
    /// New value for the setting
    /// </summary>
    public string Value { get; init; } = string.Empty;

    /// <summary>
    /// Row version for optimistic concurrency control
    /// </summary>
    public byte[]? RowVersion { get; init; }

    /// <summary>
    /// ID of the user making the change
    /// </summary>
    public Guid? UserId { get; init; }
}
