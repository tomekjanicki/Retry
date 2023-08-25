using ApiClient.Services;
using Retry.Resiliency;

namespace Retry.Services;

public sealed class WithRetryAndCircuitBreakerExternalApiClientNetStandard : IExternalApiClientNetStandard
{
    private readonly IExternalApiClientNetStandard _api;
    private readonly AsyncPolicyAndHandlerWrapper<string> _getTimeAsStringPolicyAndHandler;

    public WithRetryAndCircuitBreakerExternalApiClientNetStandard(IExternalApiClientNetStandard api, PolicyAndHandlerWrapperProvider provider, ILogger<WithRetryAndCircuitBreakerExternalApiClientNetStandard> logger)
    {
        _api = api;
        _getTimeAsStringPolicyAndHandler = provider.GetRetryAndCircuitBreakerTransientHttpRequestExceptionAsyncPolicyAndHandler<string>(logger);
    }

    public Task<string> GetTimeAsString(bool fail, CancellationToken cancellationToken) =>
        _getTimeAsStringPolicyAndHandler.ExecuteAsync((fail, _api), static (p, token) => p._api.GetTimeAsString(p.fail, token), cancellationToken);
}