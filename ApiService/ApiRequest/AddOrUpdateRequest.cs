namespace TestAssignment.ApiService.ApiRequest;

public record AddOrUpdateRequest<T> 
    (string IpAddress, string Email, string ResourceId, T Resource) : BaseRequest(IpAddress, ResourceId);