namespace TestAssignment.ApiService;

public interface IResourceProvider<T>
{
    // I didn't want to generate exceptions in case there is no element with given Id in the IResourceProvider.
    // I have slightly corrected the signature of the method
    public bool TryGetResource(string id, out T resource);

    public void AddOrUpdateResource(string id, T resource);
}