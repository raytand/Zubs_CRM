using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;
using Zubs.Application.Interfaces.Services;

namespace Zubs.Infrastructure.Cache;

public class CacheService(IDistributedCache cache, IConnectionMultiplexer redis) : ICacheService
{
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(10);

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var bytes = await cache.GetAsync(key, ct);
        return bytes is null ? default : JsonSerializer.Deserialize<T>(bytes);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry ?? DefaultExpiry
        };
        await cache.SetAsync(key, JsonSerializer.SerializeToUtf8Bytes(value), options, ct);
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
        => cache.RemoveAsync(key, ct);

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        var server = redis.GetServer(redis.GetEndPoints().First());
        var db = redis.GetDatabase();
        await foreach (var key in server.KeysAsync(pattern: $"zubs:{prefix}*"))
            await db.KeyDeleteAsync(key);
    }
}