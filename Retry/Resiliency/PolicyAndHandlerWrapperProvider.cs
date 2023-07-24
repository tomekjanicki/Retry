using Microsoft.Extensions.Options;
using OneOf;
using OneOf.Types;
using Retry.Extensions;
using Retry.Resiliency.Models;

namespace Retry.Resiliency;

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
            ResiliencyHelper.GetRetryAndCircuitBreakerExceptionAsyncPolicy<TResult, TException>(_configuration, predicate)
            : ResiliencyHelper.GetRetryAndCircuitBreakerExceptionAsyncPolicySimple<TResult, TException>(predicate);
        var handler = ResiliencyHelper.GetHandler<TResult>(logger);

        return new AsyncPolicyAndHandlerWrapper<TResult>(policy, handler);
    }

    public AsyncPolicyAndHandlerWrapper<TResult> GetCircuitBreakerExceptionAsyncPolicyAndHandler<TResult,
        TException>(ILogger logger, Func<TException, bool> predicate)
        where TException : Exception
    {
        var policy = _configuration is not null ?
            ResiliencyHelper.GetCircuitBreakerExceptionAsyncPolicy<TResult, TException>(_configuration.CircuitBreakerPolicy, predicate)
            : ResiliencyHelper.GetCircuitBreakerExceptionAsyncPolicySimple<TResult, TException>(predicate);
        var handler = ResiliencyHelper.GetHandler<TResult>(logger);

        return new AsyncPolicyAndHandlerWrapper<TResult>(policy, handler);
    }

    public AsyncPolicyAndHandlerWrapper GetCircuitBreakerExceptionAsyncPolicyAndHandler<TException>(ILogger logger, Func<TException, bool> predicate)
        where TException : Exception
    {
        var policy = _configuration is not null ?
            ResiliencyHelper.GetCircuitBreakerExceptionAsyncPolicy(_configuration.CircuitBreakerPolicy, predicate)
            : ResiliencyHelper.GetCircuitBreakerExceptionAsyncPolicySimple(predicate);
        var handler = ResiliencyHelper.GetHandler(logger);

        return new AsyncPolicyAndHandlerWrapper(policy, handler);
    }

    public AsyncPolicyAndHandlerWrapper GetRetryAndCircuitBreakerExceptionAsyncPolicyAndHandler<TException>(ILogger logger, Func<TException, bool> predicate)
        where TException : Exception
    {
        var policy = _configuration is not null ?
            ResiliencyHelper.GetRetryAndCircuitBreakerExceptionAsyncPolicy(_configuration, predicate)
            : ResiliencyHelper.GetRetryAndCircuitBreakerExceptionAsyncPolicySimple(predicate);
        var handler = ResiliencyHelper.GetHandler(logger);

        return new AsyncPolicyAndHandlerWrapper(policy, handler);
    }

    public AsyncPolicyAndHandlerWrapper<OneOf<TResult, NotFound, Error>> GetRetryAndCircuitBreakerTransientHttpRequestExceptionOrOneOfResultWithNotFoundAsyncPolicyAndHandler<TResult>(ILogger logger)
    {
        var policy = _configuration is not null ? ResiliencyHelper.GetRetryAndCircuitBreakerTransientHttpRequestExceptionOrOneOfResultWithNotFoundAsyncPolicy<TResult>(_configuration)
                : ResiliencyHelper.GetRetryAndCircuitBreakerTransientHttpRequestExceptionOrOneOfResultWithNotFoundAsyncPolicySimple<TResult>();
        var handler = ResiliencyHelper.GetHandler<OneOf<TResult, NotFound, Error>>(logger);

        return new AsyncPolicyAndHandlerWrapper<OneOf<TResult, NotFound, Error>>(policy, handler);
    }
}