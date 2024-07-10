using System.Threading.Tasks;

using TestAssignment.ApiService.Throttle;
using TestAssignment.ApiService.Providers;
using TestAssignment.ApiService.ApiRequest;
using TestAssignment.ApiService.ApiResponse;

namespace TestAssignment.ApiService;

public class ApiService<TResource> : IApiService<TResource>
{
    private readonly ThrottleService _throttleService;
    private readonly ResourceProviderCacheWrapper<TResource> _resourceProvider;

    public ApiService(ITimeProvider timeProvider,
                      ThrottleSettings throttleSettings, 
                      IResourceProvider<TResource> resourceProvider)
    {
        _throttleService = new(throttleSettings, timeProvider);
        _resourceProvider = new(resourceProvider);
    }

    public async Task<GetResponse<TResource>> GetResourceAsync(GetRequest request)
    {
        if  (!await _throttleService.CanAccessAsync(request))
        {
            return new GetResponse<TResource>(Success: false, ResourceData: default, ErrorType.TooManyRequests);
        }

        (bool isSuccess, TResource result) = await _resourceProvider.TryGetResourceAsync(request.ResourceId);

        if (!isSuccess)
        {
            return new GetResponse<TResource>(Success: false, ResourceData: default, ErrorType.ResourceNotFound);
        }

        return new GetResponse<TResource>(Success: true, ResourceData: result, null);
    }

    public async Task<AddOrUpdateResponse> AddOrUpdateResourceAsync(AddOrUpdateRequest<TResource> request)
    {
        if (!await _throttleService.CanAccessAsync(request))
        {
            return new AddOrUpdateResponse(Success: false, ErrorType.TooManyRequests);
        }

        await _resourceProvider.AddOrUpdateResourceAsync(request.ResourceId, request.Resource);

        return new AddOrUpdateResponse(Success: true, ErrorType: null);      
    }
}