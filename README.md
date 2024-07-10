Main Task Description:

Write a class implementing IApiService interface, with the following specification
- calls to IApiService methods, should be throttled according to the provided ThrottleSettings
- assume that IResourceProviders methods might be long-running so use caching (without expiration) by ResourceId
  (10 parallel IApiService.GetResource calls should trigger only 1 IResourceProvider.GetResource call)
- IApiService methods should be thread-safe for concurrent calls where GetResource for the same ResourceId can be accessed
  concurrently but AddOrUpdateResource for the same ResourceId should not, from obvious Get & Update semantics
  
Additional Details Specification:
- service should dependency inject (at least) ThrottleSettings, IResourceProvider<T>, and ITimeProvider as shown
  in ApiServiceFactory
- to get and update resource in IApiServices GetResource and AddOrUpdateResource use IResourceProvider methods
  GetResource and AddOrUpdateResource respectively
- throttling should be implemented using the provided ITimeProvider for testing purposes (as demonstrated in the attached tests)
- do not throw exceptions, rather define error type and return concrete error type with a response.Success set to false
- methods of IApiService can and will be called in parallel, so make sure that they are thread-safe
- IApiService intended lifetime is a singleton
