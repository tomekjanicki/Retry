using Microsoft.Extensions.Options;
using OneOf;
using OneOf.Types;
using Polly;
using Retry.Extensions;

namespace Retry.Services;

public sealed class WithRetryExternalApiClient : IExternalApiClient
{
    private readonly IExternalApiClient _api;
    private readonly IAsyncPolicy<string> _getTimeAsStringPolicy;
    private readonly IAsyncPolicy<IReadOnlyCollection<string>> _getItemsPolicy;
    private readonly IAsyncPolicy<OneOf<string, NotFound, Error>> _getUserFullNameByIdPolicy;

    public WithRetryExternalApiClient(IExternalApiClient api, IOptions<ConfigurationSettings> options)
    {
        _api = api;
        var configuration = options.Value.RetryAndCircuitBreakerPolicyConfiguration;
        if (configuration is null)
        {
            _getTimeAsStringPolicy = PolicyHelper.GetRetryAndCircuitBreakerExceptionAsyncPolicySimple<string, HttpRequestException>();
            _getItemsPolicy = PolicyHelper.GetRetryAndCircuitBreakerExceptionAsyncPolicySimple<IReadOnlyCollection<string>, HttpRequestException>();
            _getUserFullNameByIdPolicy = PolicyHelper.GetRetryAndCircuitBreakerOneOfResultWithNotFoundAsyncPolicySimple<string>();
        }
        else
        {
            _getTimeAsStringPolicy = PolicyHelper.GetRetryAndCircuitBreakerExceptionAsyncPolicy<string, HttpRequestException>(configuration);
            _getItemsPolicy = PolicyHelper.GetRetryAndCircuitBreakerExceptionAsyncPolicy<IReadOnlyCollection<string>, HttpRequestException>(configuration);
            _getUserFullNameByIdPolicy = PolicyHelper.GetRetryAndCircuitBreakerOneOfResultWithNotFoundAsyncPolicy<string>(configuration);
        }
    }

    public Task<string> GetTimeAsString(bool fail, CancellationToken cancellationToken) => 
        _getTimeAsStringPolicy.ExecuteAsync((fail, _api), static (p, token) => p._api.GetTimeAsString(p.fail, token), cancellationToken);

    public Task<IReadOnlyCollection<string>> GetItems(bool fail, CancellationToken cancellationToken) =>
        _getItemsPolicy.ExecuteAsync((fail, _api), static (p, token) => p._api.GetItems(p.fail, token), cancellationToken);

    public async Task<OneOf<string, NotFound, Error>> GetUserFullNameById(int id, bool fail, CancellationToken cancellationToken)
    {
        var result = await _getUserFullNameByIdPolicy.ExecuteAndCaptureAsync((fail, _api, id),
            static (p, token) => p._api.GetUserFullNameById(p.id, p.fail, token), cancellationToken);

        return result.HandleFailureAndSuccess();
    }
}