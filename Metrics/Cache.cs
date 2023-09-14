using Metrics.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Metrics;

public class Cache : ICache
{
    private readonly MemoryCache _cache;
    private readonly TimeSpan timeCache = TimeSpan.FromHours(2);

    public Cache()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
    }

    public T? TryGetValue<T>(object Key) 
    {
        if(_cache.TryGetValue(Key, out T? result))
            return result;

        return default;
    }

    public void SetValue<T>(object Key, T value) 
    {
        _cache.Set(Key, value, timeCache);
    }

    public void DeleteValue(object Key) 
    {
        _cache.Remove(Key);
    }
}
