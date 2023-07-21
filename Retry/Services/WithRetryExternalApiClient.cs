using Microsoft.Extensions.Options;
using OneOf;
using OneOf.Types;
using Retry.Extensions;

namespace Retry.Services;

public sealed class WithRetryExternalApiClient : IExternalApiClient
{
    private readonly IExternalApiClient _api;
    private readonly PolicyAndHandlerWrapper<string> _getTimeAsStringPolicyAndHandler;
    private readonly PolicyAndHandlerWrapper<IReadOnlyCollection<string>> _getItemsPolicyAndHandler;
    private readonly PolicyAndHandlerWrapper<OneOf<string, NotFound, Error>> _getUserFullNameByIdPolicyAndHandler;

    public WithRetryExternalApiClient(IExternalApiClient api, IOptions<ConfigurationSettings> options, ILogger<WithRetryExternalApiClient> logger)
    {
        _api = api;
        var configuration = options.Value.RetryAndCircuitBreakerPolicyConfiguration;
        var getTimeAsStringHandler = PolicyHelper.GetHandler<string>(logger);
        var getItemsHandler = PolicyHelper.GetHandler<IReadOnlyCollection<string>>(logger);
        var getUserFullNameByIdHandler = PolicyHelper.GetHandler<OneOf<string, NotFound, Error>>(logger);
        if (configuration is null)
        {
            var getTimeAsStringPolicy = PolicyHelper.GetRetryAndCircuitBreakerExceptionAsyncPolicySimple<string, HttpRequestException>();
            var getItemsPolicy = PolicyHelper.GetRetryAndCircuitBreakerExceptionAsyncPolicySimple<IReadOnlyCollection<string>, HttpRequestException>();
            var getUserFullNameByIdPolicy = PolicyHelper.GetRetryAndCircuitBreakerHttpRequestExceptionOrOneOfResultWithNotFoundAsyncPolicySimple<string>();
            _getTimeAsStringPolicyAndHandler = new PolicyAndHandlerWrapper<string>(getTimeAsStringPolicy, getTimeAsStringHandler);
            _getItemsPolicyAndHandler = new PolicyAndHandlerWrapper<IReadOnlyCollection<string>>(getItemsPolicy, getItemsHandler);
            _getUserFullNameByIdPolicyAndHandler = new PolicyAndHandlerWrapper<OneOf<string, NotFound, Error>>(getUserFullNameByIdPolicy, getUserFullNameByIdHandler);
        }
        else
        {
            var getTimeAsStringPolicy = PolicyHelper.GetRetryAndCircuitBreakerExceptionAsyncPolicy<string, HttpRequestException>(configuration);
            var getItemsPolicy = PolicyHelper.GetRetryAndCircuitBreakerExceptionAsyncPolicy<IReadOnlyCollection<string>, HttpRequestException>(configuration);
            var getUserFullNameByIdPolicy = PolicyHelper.GetRetryAndCircuitBreakerHttpRequestExceptionOrOneOfResultWithNotFoundAsyncPolicy<string>(configuration);
            _getTimeAsStringPolicyAndHandler = new PolicyAndHandlerWrapper<string>(getTimeAsStringPolicy, getTimeAsStringHandler);
            _getItemsPolicyAndHandler = new PolicyAndHandlerWrapper<IReadOnlyCollection<string>>(getItemsPolicy, getItemsHandler);
            _getUserFullNameByIdPolicyAndHandler = new PolicyAndHandlerWrapper<OneOf<string, NotFound, Error>>(getUserFullNameByIdPolicy, getUserFullNameByIdHandler);
        }
    }

    public Task<string> GetTimeAsString(bool fail, CancellationToken cancellationToken) =>
        _getTimeAsStringPolicyAndHandler.ExecuteAsync((fail, _api), static (p, token) => p._api.GetTimeAsString(p.fail, token), cancellationToken);

    public Task<IReadOnlyCollection<string>> GetItems(bool fail, CancellationToken cancellationToken) =>
        _getItemsPolicyAndHandler.ExecuteAsync((fail, _api), static (p, token) => p._api.GetItems(p.fail, token), cancellationToken);

    public async Task<OneOf<string, NotFound, Error>> GetUserFullNameById(int id, bool fail, CancellationToken cancellationToken)
    {
        var result = await _getUserFullNameByIdPolicyAndHandler.ExecuteAndCaptureAsync((fail, _api, id),
            static (p, token) => p._api.GetUserFullNameById(p.id, p.fail, token), cancellationToken);

        return result.HandleFailureAndSuccess();
    }
}