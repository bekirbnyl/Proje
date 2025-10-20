namespace Sinema.Domain.Interfaces.Services;

/// <summary>
/// Provides access to current time for testability
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets the current UTC time
    /// </summary>
    DateTime UtcNow { get; }
}
