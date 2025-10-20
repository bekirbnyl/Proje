namespace Sinema.Application.Abstractions.Idempotency;

/// <summary>
/// Store for managing idempotency keys to prevent duplicate operations
/// </summary>
public interface IIdempotencyStore
{
    /// <summary>
    /// Stores a result for the given idempotency key
    /// </summary>
    /// <param name="key">The idempotency key</param>
    /// <param name="result">The result to store</param>
    /// <param name="expiryMinutes">How long to keep the result (default 60 minutes)</param>
    Task StoreResultAsync<T>(string key, T result, int expiryMinutes = 60);

    /// <summary>
    /// Retrieves a previously stored result for the given idempotency key
    /// </summary>
    /// <param name="key">The idempotency key</param>
    /// <returns>The stored result, or null if not found or expired</returns>
    Task<T?> GetResultAsync<T>(string key) where T : class;

    /// <summary>
    /// Checks if an idempotency key exists (regardless of result type)
    /// </summary>
    /// <param name="key">The idempotency key</param>
    /// <returns>True if the key exists and hasn't expired</returns>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// Removes a stored result for the given idempotency key
    /// </summary>
    /// <param name="key">The idempotency key</param>
    Task RemoveAsync(string key);
}
