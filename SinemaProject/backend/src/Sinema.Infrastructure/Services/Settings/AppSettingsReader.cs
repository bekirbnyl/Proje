using Microsoft.Extensions.Configuration;
using Sinema.Application.Abstractions.Settings;
using Sinema.Domain.Entities;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Infrastructure.Services.Settings;

/// <summary>
/// Implementation of settings reader that checks database first, then falls back to configuration
/// </summary>
public class AppSettingsReader : IAppSettingsReader
{
    private readonly IConfiguration _configuration;
    private readonly ISettingsRepository _settingsRepository;

    public AppSettingsReader(IConfiguration configuration, ISettingsRepository settingsRepository)
    {
        _configuration = configuration;
        _settingsRepository = settingsRepository;
    }

    public async Task<string?> GetHalkGunuAsync()
    {
        return await GetSettingAsync(Setting.Keys.HalkGunu);
    }

    public async Task<decimal?> GetBaseTicketPriceAsync()
    {
        return await GetDecimalSettingAsync("BaseTicketPrice");
    }

    public async Task<string?> GetSettingAsync(string key)
    {
        try
        {
            // First, try to get from database
            var setting = await _settingsRepository.GetByKeyAsync(key);
            
            if (setting != null && !string.IsNullOrEmpty(setting.Value))
            {
                return setting.Value;
            }
        }
        catch
        {
            // If database access fails, fall back to configuration
        }

        // Fall back to configuration
        return _configuration[key];
    }

    public async Task<decimal?> GetDecimalSettingAsync(string key)
    {
        var value = await GetSettingAsync(key);
        
        if (string.IsNullOrEmpty(value))
            return null;

        if (decimal.TryParse(value, out var result))
            return result;

        return null;
    }

    public async Task<bool?> GetBoolSettingAsync(string key)
    {
        var value = await GetSettingAsync(key);
        
        if (string.IsNullOrEmpty(value))
            return null;

        if (bool.TryParse(value, out var result))
            return result;

        // Handle common string representations
        var normalizedValue = value.ToLowerInvariant();
        return normalizedValue switch
        {
            "1" or "yes" or "true" or "on" => true,
            "0" or "no" or "false" or "off" => false,
            _ => null
        };
    }

    public async Task<int?> GetIntSettingAsync(string key)
    {
        var value = await GetSettingAsync(key);
        
        if (string.IsNullOrEmpty(value))
            return null;

        if (int.TryParse(value, out var result))
            return result;

        return null;
    }
}
