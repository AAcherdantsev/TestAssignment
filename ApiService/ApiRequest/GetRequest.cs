namespace TestAssignment.ApiService.ApiRequest;

public record GetRequest(string IpAddress, string Email, string ResourceId) 
    : BaseRequest(IpAddress, ResourceId);