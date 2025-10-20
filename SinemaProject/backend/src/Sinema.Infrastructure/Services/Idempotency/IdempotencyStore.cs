using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Sinema.Application.Abstractions.Idempotency;
using System.Text.Json;

namespace Sinema.Infrastructure.Services.Idempotency;

/// <summary>
/// In-memory implementation of idempotency store using IMemoryCache
/// In production, this should be replaced with a distributed cache like Redis
/// </summary>
public class IdempotencyStore : IIdempotencyStore
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<IdempotencyStore> _logger;
    private const string KeyPrefix = "idempotency:";

    public IdempotencyStore(IMemoryCache cache, ILogger<IdempotencyStore> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task StoreResultAsync<T>(string key, T result, int expiryMinutes = 60)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Idempotency key cannot be null or empty", nameof(key));
        }

        var cacheKey = KeyPrefix + key;
        var serializedResult = JsonSerializer.Serialize(result);
        
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expiryMinutes),
            Priority = CacheItemPriority.Normal
        };

        _cache.Set(cacheKey, serializedResult, cacheOptions);
        
        _logger.LogDebug("Stored idempotency result for key: {Key}, expires in {ExpiryMinutes} minutes", 
            key, expiryMinutes);

        return Task.CompletedTask;
    }

    public Task<T?> GetResultAsync<T>(string key) where T : class
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return Task.FromResult<T?>(null);
        }

        var cacheKey = KeyPrefix + key;
        
        if (_cache.TryGetValue(cacheKey, out var cachedValue) && cachedValue is string serializedResult)
        {
            try
            {
                var result = JsonSerializer.Deserialize<T>(serializedResult);
                _logger.LogDebug("Retrieved idempotency result for key: {Key}", key);
                return Task.FromResult(result);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize idempotency result for key: {Key}", key);
                // Remove corrupted entry
                _cache.Remove(cacheKey);
            }
        }

        return Task.FromResult<T?>(null);
    }

    public Task<bool> ExistsAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return Task.FromResult(false);
        }

        var cacheKey = KeyPrefix + key;
        var exists = _cache.TryGetValue(cacheKey, out _);
        
        return Task.FromResult(exists);
    }

    public Task RemoveAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return Task.CompletedTask;
        }

        var cacheKey = KeyPrefix + key;
        _cache.Remove(cacheKey);
        
        _logger.LogDebug("Removed idempotency result for key: {Key}", key);
        
        return Task.CompletedTask;
    }
}
