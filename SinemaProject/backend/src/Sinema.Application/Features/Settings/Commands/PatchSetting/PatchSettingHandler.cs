using MediatR;
using Sinema.Application.DTOs.Settings;
using Sinema.Domain.Interfaces.Services;

namespace Sinema.Application.Features.Settings.Commands.PatchSetting;

/// <summary>
/// Handler for patching a single setting with concurrency control
/// </summary>
public class PatchSettingHandler : IRequestHandler<PatchSettingCommand, SettingDto>
{
    private readonly ISettingsService _settingsService;

    public PatchSettingHandler(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<SettingDto> Handle(PatchSettingCommand request, CancellationToken cancellationToken)
    {
        // Update setting through service (includes validation, concurrency control, and change logging)
        var updatedSetting = await _settingsService.PatchAsync(
            request.Key, 
            request.Value, 
            request.RowVersion, 
            request.UserId, 
            cancellationToken);

        // Convert to DTO
        return new SettingDto
        {
            Key = updatedSetting.Key,
            Value = updatedSetting.Value,
            Description = updatedSetting.Description,
            RowVersion = Convert.ToBase64String(updatedSetting.RowVersion),
            UpdatedAt = updatedSetting.UpdatedAt
        };
    }
}
