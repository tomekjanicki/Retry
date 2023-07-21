﻿using Microsoft.Extensions.Options;
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
        if (configuration is null)
        {
            _getTimeAsStringPolicyAndHandler = PolicyAndHandlerWrapperHelper.GetRetryAndCircuitBreakerExceptionAsyncPolicyAndHandlerSimple<string, HttpRequestException>(logger);
            _getItemsPolicyAndHandler = PolicyAndHandlerWrapperHelper.GetRetryAndCircuitBreakerExceptionAsyncPolicyAndHandlerSimple<IReadOnlyCollection<string>, HttpRequestException>(logger);
            _getUserFullNameByIdPolicyAndHandler = PolicyAndHandlerWrapperHelper.GetRetryAndCircuitBreakerHttpRequestExceptionOrOneOfResultWithNotFoundAsyncPolicyAndHandlerSimple<string>(logger);
        }
        else
        {
            _getTimeAsStringPolicyAndHandler = PolicyAndHandlerWrapperHelper.GetRetryAndCircuitBreakerExceptionAsyncPolicyAndHandler<string, HttpRequestException>(configuration, logger);
            _getItemsPolicyAndHandler = PolicyAndHandlerWrapperHelper.GetRetryAndCircuitBreakerExceptionAsyncPolicyAndHandler<IReadOnlyCollection<string>, HttpRequestException>(configuration, logger);
            _getUserFullNameByIdPolicyAndHandler = PolicyAndHandlerWrapperHelper.GetRetryAndCircuitBreakerHttpRequestExceptionOrOneOfResultWithNotFoundAsyncPolicyAndHandler<string>(configuration, logger);
        }
    }

    public Task<string> GetTimeAsString(bool fail, CancellationToken cancellationToken) =>
        _getTimeAsStringPolicyAndHandler.ExecuteAsync((fail, _api), static (p, token) => p._api.GetTimeAsString(p.fail, token), cancellationToken);

    public Task<IReadOnlyCollection<string>> GetItems(bool fail, CancellationToken cancellationToken) =>
        _getItemsPolicyAndHandler.ExecuteAsync((fail, _api), static (p, token) => p._api.GetItems(p.fail, token), cancellationToken);

    public Task<OneOf<string, NotFound, Error>> GetUserFullNameById(int id, bool fail, CancellationToken cancellationToken) =>
        _getUserFullNameByIdPolicyAndHandler.ExecuteAsResult((fail, _api, id), static (p, token) => p._api.GetUserFullNameById(p.id, p.fail, token), cancellationToken);
}