using TestAssignment.ApiService;
using TestAssignment.ApiService.Providers;
using TestAssignment.ApiService.Throttle;

namespace TestAssignment;

public class Program
{
    static ITimeProvider timeProvider = new ApiService.Providers.TimeProvider();

    static IResourceProvider<int> resourceProvider = new ResourceProvider<int>();

    static ThrottleSettings throttleSettings = new ThrottleSettings()
    {
        MaxRequestsPerIp = 3,
        BanTimeOut = TimeSpan.FromSeconds(15),
        ThrottleInterval = TimeSpan.FromSeconds(5),
    };

    static IApiService<int> api = ApiServiceFactory.CreateApiService(throttleSettings, resourceProvider, timeProvider);

    public static void Main(string[] args)
    {
        // some kind of use of IApiService...
    }
}