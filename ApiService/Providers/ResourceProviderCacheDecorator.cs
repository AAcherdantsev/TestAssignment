using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace TestAssignment.ApiService.Providers;


// I made a wrapper for ResourceProvider 
// I don't use caching of negative results, although it could be added. 
// This can lead to unnecessary time costs if there are many requests for resources that do not exist.
// Since the assignment says that I can't use ExpirationTime, I use the cache size limit.

// I assume that the implementation of the IResourceProvider interface is thread-safe
// Therefore, I can request resources with different IDs at the same time.
// I added a dictionary with semaphores so that no more than 1 thread can access 
// the ResourceProvider with a specific resource ID at any given time.

public class ResourceProviderCacheWrapper<T>
{
    const int CACHE_SIZE = 100;
    const int ELEMENT_SIZE = 1;

    private readonly MemoryCache _cache;
    private readonly IResourceProvider<T> _resourceProvider;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks;

    public ResourceProviderCacheWrapper(IResourceProvider<T> resourceProvider)
    {
        _resourceProvider = resourceProvider;

        _locks = new ConcurrentDictionary<string, SemaphoreSlim>(Environment.ProcessorCount, CACHE_SIZE);

        _cache = new MemoryCache(new MemoryCacheOptions()
        {
            SizeLimit = CACHE_SIZE
        });
    }

    public async Task<(bool success, T resource)> TryGetResourceAsync(string id)
    {
        if (_cache.TryGetValue(id, out T resource))
        {
            return (true, resource);
        }

        bool isSuccess = false;

        SemaphoreSlim semaphoreLock = _locks.GetOrAdd(id, _ => new SemaphoreSlim(1, 1));

        await semaphoreLock.WaitAsync();

        try
        {
            isSuccess = _cache.TryGetValue(id, out resource);

            if (!isSuccess)
            {
                isSuccess = _resourceProvider.TryGetResource(id, out resource);

                if (isSuccess)
                {
                    _cache.Set(id, resource, CreateCacheOption());
                }
            }
        }
        finally
        {
            semaphoreLock.Release();
        }

        return (isSuccess, resource);
    }

    public async Task AddOrUpdateResourceAsync(string id, T resource)
    {
        SemaphoreSlim semaphoreLock = _locks.GetOrAdd(id, _ => new SemaphoreSlim(1, 1));

        await semaphoreLock.WaitAsync();

        try
        {
            _resourceProvider.AddOrUpdateResource(id, resource);

            if (_cache.TryGetValue(id, out _))
            {
                _cache.Set(id, resource);
            }
        }
        finally
        {
            semaphoreLock.Release();
        }
    }

    private MemoryCacheEntryOptions CreateCacheOption()
    {
        MemoryCacheEntryOptions cacheOptions = new()
        {
            Size = ELEMENT_SIZE,
        };

        PostEvictionCallbackRegistration callbackRegistration = new()
        {
            EvictionCallback = OnElementEvicted
        };

        cacheOptions.PostEvictionCallbacks.Add(callbackRegistration);

        return cacheOptions;
    }

    private void OnElementEvicted(object key, object value, EvictionReason reason, object state)
    {
        _locks.TryRemove((string)key, out _);
    }
}