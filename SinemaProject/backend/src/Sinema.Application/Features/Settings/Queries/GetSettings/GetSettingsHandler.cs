using MediatR;
using Sinema.Application.DTOs.Settings;
using Sinema.Domain.Interfaces.Services;

namespace Sinema.Application.Features.Settings.Queries.GetSettings;

/// <summary>
/// Handler for getting settings with whitelist filtering
/// </summary>
public class GetSettingsHandler : IRequestHandler<GetSettingsQuery, Dictionary<string, SettingDto>>
{
    private readonly ISettingsService _settingsService;

    public GetSettingsHandler(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<Dictionary<string, SettingDto>> Handle(GetSettingsQuery request, CancellationToken cancellationToken)
    {
        // Get settings from service (already filtered by whitelist)
        var settings = await _settingsService.GetAsync(request.Keys, cancellationToken);
        
        // Convert to DTOs
        var result = new Dictionary<string, SettingDto>();
        
        foreach (var kvp in settings)
        {
            result[kvp.Key] = new SettingDto
            {
                Key = kvp.Key,
                Value = kvp.Value,
                Description = null, // Description not currently exposed in this endpoint
                RowVersion = null,  // RowVersion not needed for read operations
                UpdatedAt = null    // UpdatedAt not needed for this simple view
            };
        }

        return result;
    }
}
