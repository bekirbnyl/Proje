using MediatR;
using Sinema.Application.DTOs.Settings;
using Sinema.Domain.Interfaces.Services;

namespace Sinema.Application.Features.Settings.Commands.UpdateSettings;

/// <summary>
/// Handler for updating multiple settings
/// </summary>
public class UpdateSettingsHandler : IRequestHandler<UpdateSettingsCommand, Dictionary<string, SettingDto>>
{
    private readonly ISettingsService _settingsService;

    public UpdateSettingsHandler(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<Dictionary<string, SettingDto>> Handle(UpdateSettingsCommand request, CancellationToken cancellationToken)
    {
        // Update settings through service (includes validation and change logging)
        var updatedSettings = await _settingsService.UpdateAsync(request.Settings, request.UserId, cancellationToken);

        // Convert to DTOs
        var result = new Dictionary<string, SettingDto>();
        
        foreach (var kvp in updatedSettings)
        {
            var setting = kvp.Value;
            result[kvp.Key] = new SettingDto
            {
                Key = setting.Key,
                Value = setting.Value,
                Description = setting.Description,
                RowVersion = Convert.ToBase64String(setting.RowVersion),
                UpdatedAt = setting.UpdatedAt
            };
        }

        return result;
    }
}
