using Sinema.Domain.Interfaces.Services;

namespace Sinema.Infrastructure.Services.Clock;

/// <summary>
/// System implementation of IClock that returns real system time
/// </summary>
public class SystemClock : IClock
{
    /// <summary>
    /// Gets the current UTC time from the system
    /// </summary>
    public DateTime UtcNow => DateTime.UtcNow;
}
