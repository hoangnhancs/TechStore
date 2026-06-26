using System.Text.Json;
using StackExchange.Redis;

namespace SearchService.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _db;
    private readonly IServer _server;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public RedisCacheService(IConnectionMultiplexer mux)
    {
        _db = mux.GetDatabase();
        _server = mux.GetServer(mux.GetEndPoints().First());
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty) return default;
        return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json, ttl ?? TimeSpan.FromMinutes(5));
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        await _db.KeyDeleteAsync(key);
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        var keys = _server.KeysAsync(pattern: $"{prefix}*").WithCancellation(ct);
        await foreach (var key in keys)
            await _db.KeyDeleteAsync(key);
    }
}
