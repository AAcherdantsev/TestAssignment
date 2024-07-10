using System.Collections.Concurrent;

namespace TestAssignment.ApiService.Providers;

public class ResourceProvider<T> : IResourceProvider<T>
{
    private readonly ConcurrentDictionary<string, T> _resourceDictionary;

    public ResourceProvider()
    {
        _resourceDictionary = new ConcurrentDictionary<string, T>();
    }

    public bool TryGetResource(string id, out T resource)
    {
        return _resourceDictionary.TryGetValue(id, out resource);
    }

    public void AddOrUpdateResource(string id, T resource)
    {
        _resourceDictionary[id] = resource;
    }
}