using ApiClient.Services;
using Retry.Resiliency;

namespace Retry.Services;

public sealed class WithRetryAndCircuitBreakerExternalApiClientNetStandard : IExternalApiClientNetStandard
{
    private readonly IExternalApiClientNetStandard _api;
    private readonly ResiliencePipelineWrapper<string> _getTimeAsStringResiliencePipelineWrapper;

    public WithRetryAndCircuitBreakerExternalApiClientNetStandard(IExternalApiClientNetStandard api, ResiliencePipelineWrapperProvider provider, ILogger<WithRetryAndCircuitBreakerExternalApiClientNetStandard> logger)
    {
        _api = api;
        _getTimeAsStringResiliencePipelineWrapper = provider.GetRetryAndCircuitBreakerTransientHttpRequestExceptionPipelineWrapper<string>(logger);
    }

    public Task<string> GetTimeAsString(bool fail, CancellationToken cancellationToken = default) =>
        _getTimeAsStringResiliencePipelineWrapper.ExecuteAsync((fail, _api), static (p, token) => p._api.GetTimeAsString(p.fail, token), cancellationToken);
}