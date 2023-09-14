using OneOf;
using OneOf.Types;
using Retry.Extensions;
using Retry.Resiliency;

namespace Retry.Services;

public sealed class WithRetryAndCircuitBreakerExternalApiClient : IExternalApiClient
{
    private readonly IExternalApiClient _api;
    private readonly AsyncPolicyAndHandlerWrapper<string> _getTimeAsStringPolicyAndHandler;
    private readonly AsyncPolicyAndHandlerWrapper<IReadOnlyCollection<string>> _getItemsPolicyAndHandler;
    private readonly AsyncPolicyAndHandlerWrapper<OneOf<string, NotFound, ApiError>> _getUserFullNameByIdPolicyAndHandler;

    public WithRetryAndCircuitBreakerExternalApiClient(IExternalApiClient api, PolicyAndHandlerWrapperProvider provider, ILogger<WithRetryAndCircuitBreakerExternalApiClient> logger)
    {
        _api = api;
        _getTimeAsStringPolicyAndHandler = provider.GetRetryAndCircuitBreakerTransientHttpRequestExceptionAsyncPolicyAndHandler<string>(logger);
        _getItemsPolicyAndHandler = provider.GetRetryAndCircuitBreakerTransientHttpRequestExceptionAsyncPolicyAndHandler<IReadOnlyCollection<string>>(logger);
        _getUserFullNameByIdPolicyAndHandler = provider.GetRetryAndCircuitBreakerOneOfResultWithNotFoundAsyncPolicyAndHandler<string>(logger);
    }

    public Task<string> GetTimeAsString(bool fail, CancellationToken cancellationToken = default) =>
        _getTimeAsStringPolicyAndHandler.ExecuteAsync((fail, _api), static (p, token) => p._api.GetTimeAsString(p.fail, token), cancellationToken);

    public Task<IReadOnlyCollection<string>> GetItems(bool fail, CancellationToken cancellationToken = default) =>
        _getItemsPolicyAndHandler.ExecuteAsync((fail, _api), static (p, token) => p._api.GetItems(p.fail, token), cancellationToken);

    public Task<OneOf<string, NotFound, ApiError>> GetUserFullNameById(int id, bool fail, CancellationToken cancellationToken = default) =>
        _getUserFullNameByIdPolicyAndHandler.ExecuteAsResult((fail, _api, id), Constants.CircuitOpenApiError, PolicyAndHandlerWrapperExtensions.CircuitOpenPredicate, static (p, token) => p._api.GetUserFullNameById(p.id, p.fail, token), cancellationToken);
}