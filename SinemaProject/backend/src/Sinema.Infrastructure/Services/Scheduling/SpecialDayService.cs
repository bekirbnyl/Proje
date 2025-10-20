using Sinema.Application.Abstractions.Scheduling;
using Sinema.Domain.Entities;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Infrastructure.Services.Scheduling;

/// <summary>
/// Implementation of special day service
/// </summary>
public class SpecialDayService : ISpecialDayService
{
    private readonly ISettingsRepository _settingsRepository;

    public SpecialDayService(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    /// <inheritdoc />
    public async Task<bool> IsHalkGunuAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var halkGunuSettingEntity = await _settingsRepository.GetByKeyAsync(Setting.Keys.HalkGunu, cancellationToken);
        var halkGunuSetting = halkGunuSettingEntity?.Value;
        
        if (string.IsNullOrEmpty(halkGunuSetting))
        {
            return false;
        }

        // Parse the setting value to get the day of week
        if (Enum.TryParse<DayOfWeek>(halkGunuSetting, true, out var halkGunuDay))
        {
            return date.DayOfWeek == halkGunuDay;
        }

        return false;
    }
}
