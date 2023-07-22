using OneOf;
using OneOf.Types;

namespace Retry;

public static class PolicyAndHandlerWrapperHelper
{

    public static PolicyAndHandlerWrapper<TResult> GetRetryAndCircuitBreakerTransientHttpRequestExceptionAsyncPolicyAndHandlerSimple<TResult>(ILogger logger) =>
        GetRetryAndCircuitBreakerExceptionAsyncPolicyAndHandlerSimple<TResult, HttpRequestException>(logger, static exception => exception.ShouldHandleTransientHttpRequestException());

    public static PolicyAndHandlerWrapper<TResult> GetRetryAndCircuitBreakerTransientHttpRequestExceptionAsyncPolicyAndHandler<TResult>(RetryAndCircuitBreakerPolicyConfiguration configuration, ILogger logger) =>
        GetRetryAndCircuitBreakerExceptionAsyncPolicyAndHandler<TResult, HttpRequestException>(configuration, logger, static exception => exception.ShouldHandleTransientHttpRequestException());

    public static PolicyAndHandlerWrapper<TResult> GetRetryAndCircuitBreakerExceptionAsyncPolicyAndHandlerSimple<TResult,
        TException>(ILogger logger, Func<TException, bool> predicate)
        where TException : Exception
    {
        var policy = PolicyHelper.GetRetryAndCircuitBreakerExceptionAsyncPolicySimple<TResult, TException>(predicate);
        var handler = PolicyHelper.GetHandler<TResult>(logger);

        return new PolicyAndHandlerWrapper<TResult>(policy, handler);
    }

    public static PolicyAndHandlerWrapper<TResult> GetRetryAndCircuitBreakerExceptionAsyncPolicyAndHandler<TResult,
        TException>(RetryAndCircuitBreakerPolicyConfiguration configuration, ILogger logger, Func<TException, bool> predicate)
        where TException : Exception
    {
        var policy = PolicyHelper.GetRetryAndCircuitBreakerExceptionAsyncPolicy<TResult, TException>(configuration, predicate);
        var handler = PolicyHelper.GetHandler<TResult>(logger);

        return new PolicyAndHandlerWrapper<TResult>(policy, handler);
    }

    public static PolicyAndHandlerWrapper<OneOf<TResult, NotFound, Error>> GetRetryAndCircuitBreakerTransientHttpRequestExceptionOrOneOfResultWithNotFoundAsyncPolicyAndHandlerSimple<TResult>(ILogger logger)
    {
        var policy = PolicyHelper.GetRetryAndCircuitBreakerTransientHttpRequestExceptionOrOneOfResultWithNotFoundAsyncPolicySimple<TResult>();
        var handler = PolicyHelper.GetHandler<OneOf<TResult, NotFound, Error>>(logger);

        return new PolicyAndHandlerWrapper<OneOf<TResult, NotFound, Error>>(policy, handler);
    }

    public static PolicyAndHandlerWrapper<OneOf<TResult, NotFound, Error>> GetRetryAndCircuitBreakerTransientHttpRequestExceptionOrOneOfResultWithNotFoundAsyncPolicyAndHandler<TResult>(RetryAndCircuitBreakerPolicyConfiguration configuration, ILogger logger)
    {
        var policy = PolicyHelper.GetRetryAndCircuitBreakerTransientHttpRequestExceptionOrOneOfResultWithNotFoundAsyncPolicy<TResult>(configuration);
        var handler = PolicyHelper.GetHandler<OneOf<TResult, NotFound, Error>>(logger);

        return new PolicyAndHandlerWrapper<OneOf<TResult, NotFound, Error>>(policy, handler);
    }
}