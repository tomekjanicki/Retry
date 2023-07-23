using Microsoft.Extensions.Options;
using OneOf;
using OneOf.Types;
using Retry.Extensions;
using Retry.Infrastructure.Models;

namespace Retry.Infrastructure;

public sealed class PolicyAndHandlerWrapperProvider
{
    private readonly RetryAndCircuitBreakerPolicyConfiguration? _configuration;

    public PolicyAndHandlerWrapperProvider(IOptions<ConfigurationSettings> options) =>
        _configuration = options.Value.RetryAndCircuitBreakerPolicyConfiguration;

    public AsyncPolicyAndHandlerWrapper<TResult> GetRetryAndCircuitBreakerTransientHttpRequestExceptionAsyncPolicyAndHandler<TResult>(ILogger logger) =>
        GetRetryAndCircuitBreakerExceptionAsyncPolicyAndHandler<TResult, HttpRequestException>(logger, static exception => exception.ShouldHandleTransientHttpRequestException());

    public AsyncPolicyAndHandlerWrapper<TResult> GetRetryAndCircuitBreakerExceptionAsyncPolicyAndHandler<TResult,
        TException>(ILogger logger, Func<TException, bool> predicate)
        where TException : Exception
    {
        var policy = _configuration is not null ?
            PolicyHelper.GetRetryAndCircuitBreakerExceptionAsyncPolicy<TResult, TException>(_configuration, predicate)
            : PolicyHelper.GetRetryAndCircuitBreakerExceptionAsyncPolicySimple<TResult, TException>(predicate);
        var handler = PolicyHelper.GetHandler<TResult>(logger);

        return new AsyncPolicyAndHandlerWrapper<TResult>(policy, handler);
    }

    public AsyncPolicyAndHandlerWrapper<TResult> GetCircuitBreakerExceptionAsyncPolicyAndHandler<TResult,
        TException>(ILogger logger, Func<TException, bool> predicate)
        where TException : Exception
    {
        var policy = _configuration is not null ?
            PolicyHelper.GetCircuitBreakerExceptionAsyncPolicy<TResult, TException>(_configuration.CircuitBreakerPolicy, predicate)
            : PolicyHelper.GetCircuitBreakerExceptionAsyncPolicySimple<TResult, TException>(predicate);
        var handler = PolicyHelper.GetHandler<TResult>(logger);

        return new AsyncPolicyAndHandlerWrapper<TResult>(policy, handler);
    }

    public AsyncPolicyAndHandlerWrapper GetCircuitBreakerExceptionAsyncPolicyAndHandler<TException>(ILogger logger, Func<TException, bool> predicate)
        where TException : Exception
    {
        var policy = _configuration is not null ?
            PolicyHelper.GetCircuitBreakerExceptionAsyncPolicy(_configuration.CircuitBreakerPolicy, predicate)
            : PolicyHelper.GetCircuitBreakerExceptionAsyncPolicySimple(predicate);
        var handler = PolicyHelper.GetHandler(logger);

        return new AsyncPolicyAndHandlerWrapper(policy, handler);
    }

    public AsyncPolicyAndHandlerWrapper GetRetryAndCircuitBreakerExceptionAsyncPolicyAndHandler<TException>(ILogger logger, Func<TException, bool> predicate)
        where TException : Exception
    {
        var policy = _configuration is not null ?
            PolicyHelper.GetRetryAndCircuitBreakerExceptionAsyncPolicy(_configuration, predicate)
            : PolicyHelper.GetRetryAndCircuitBreakerExceptionAsyncPolicySimple(predicate);
        var handler = PolicyHelper.GetHandler(logger);

        return new AsyncPolicyAndHandlerWrapper(policy, handler);
    }

    public AsyncPolicyAndHandlerWrapper<OneOf<TResult, NotFound, Error>> GetRetryAndCircuitBreakerTransientHttpRequestExceptionOrOneOfResultWithNotFoundAsyncPolicyAndHandler<TResult>(ILogger logger)
    {
        var policy = _configuration is not null ? PolicyHelper.GetRetryAndCircuitBreakerTransientHttpRequestExceptionOrOneOfResultWithNotFoundAsyncPolicy<TResult>(_configuration)
                : PolicyHelper.GetRetryAndCircuitBreakerTransientHttpRequestExceptionOrOneOfResultWithNotFoundAsyncPolicySimple<TResult>();
        var handler = PolicyHelper.GetHandler<OneOf<TResult, NotFound, Error>>(logger);

        return new AsyncPolicyAndHandlerWrapper<OneOf<TResult, NotFound, Error>>(policy, handler);
    }
}