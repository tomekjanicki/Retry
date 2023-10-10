using OneOf;
using OneOf.Types;
using Retry.Extensions;
using Retry.Resiliency;

namespace Retry.Services;

public sealed class WithRetryAndCircuitBreakerExternalApiClient : IExternalApiClient
{
    private readonly IExternalApiClient _api;
    private readonly ResiliencePipelineWrapper<string> _getTimeAsStringResiliencePipelineWrapper;
    private readonly ResiliencePipelineWrapper<IReadOnlyCollection<string>> _getItemsResiliencePipelineWrapper;
    private readonly ResiliencePipelineWrapper<OneOf<string, NotFound, ApiError>> _getUserFullNameByIdResiliencePipelineWrapper;

    public WithRetryAndCircuitBreakerExternalApiClient(IExternalApiClient api, ResiliencePipelineWrapperProvider provider, ILogger<WithRetryAndCircuitBreakerExternalApiClient> logger)
    {
        _api = api;
        _getTimeAsStringResiliencePipelineWrapper = provider.GetRetryAndCircuitBreakerTransientHttpRequestExceptionPipelineWrapper<string>(logger);
        _getItemsResiliencePipelineWrapper = provider.GetRetryAndCircuitBreakerTransientHttpRequestExceptionPipelineWrapper<IReadOnlyCollection<string>>(logger);
        _getUserFullNameByIdResiliencePipelineWrapper = provider.GetRetryAndCircuitBreakerOneOfResultWithNotFoundPipelineWrapper<string>(logger);
    }

    public Task<string> GetTimeAsString(bool fail, CancellationToken cancellationToken = default) =>
        _getTimeAsStringResiliencePipelineWrapper.ExecuteAsync((fail, _api), static (p, token) => p._api.GetTimeAsString(p.fail, token), cancellationToken);

    public Task<IReadOnlyCollection<string>> GetItems(bool fail, CancellationToken cancellationToken = default) =>
        _getItemsResiliencePipelineWrapper.ExecuteAsync((fail, _api), static (p, token) => p._api.GetItems(p.fail, token), cancellationToken);

    public Task<OneOf<string, NotFound, ApiError>> GetUserFullNameById(int id, bool fail, CancellationToken cancellationToken = default) =>
        _getUserFullNameByIdResiliencePipelineWrapper.ExecuteAsResult((fail, _api, id), Constants.CircuitOpenApiError, ResiliencePipelineWrapperExtensions.CircuitOpenPredicate, static (p, token) => p._api.GetUserFullNameById(p.id, p.fail, token), cancellationToken);
}