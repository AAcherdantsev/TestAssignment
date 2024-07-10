using System.Threading.Tasks;

using TestAssignment.ApiService.ApiRequest;
using TestAssignment.ApiService.ApiResponse;

namespace TestAssignment.ApiService;

// I added the "Async" suffix to each function as they are executed asynchronously
public interface IApiService<TResource>
{
    Task<GetResponse<TResource>> GetResourceAsync(GetRequest request);

    Task<AddOrUpdateResponse> AddOrUpdateResourceAsync(AddOrUpdateRequest<TResource> request);
}