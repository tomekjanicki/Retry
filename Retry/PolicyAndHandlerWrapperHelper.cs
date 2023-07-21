using OneOf;
using OneOf.Types;

namespace Retry;

public static class PolicyAndHandlerWrapperHelper
{
    public static PolicyAndHandlerWrapper<TResult> GetRetryAndCircuitBreakerExceptionAsyncPolicyAndHandlerSimple<TResult,
        TException>(ILogger logger)
        where TException : Exception
    {
        var policy = PolicyHelper.GetRetryAndCircuitBreakerExceptionAsyncPolicySimple<TResult, TException>();
        var handler = PolicyHelper.GetHandler<TResult>(logger);

        return new PolicyAndHandlerWrapper<TResult>(policy, handler);
    }

    public static PolicyAndHandlerWrapper<TResult> GetRetryAndCircuitBreakerExceptionAsyncPolicyAndHandler<TResult,
        TException>(RetryAndCircuitBreakerPolicyConfiguration configuration, ILogger logger)
        where TException : Exception
    {
        var policy = PolicyHelper.GetRetryAndCircuitBreakerExceptionAsyncPolicy<TResult, TException>(configuration);
        var handler = PolicyHelper.GetHandler<TResult>(logger);

        return new PolicyAndHandlerWrapper<TResult>(policy, handler);
    }

    public static PolicyAndHandlerWrapper<OneOf<TResult, NotFound, Error>> GetRetryAndCircuitBreakerHttpRequestExceptionOrOneOfResultWithNotFoundAsyncPolicyAndHandlerSimple<TResult>(ILogger logger)
    {
        var policy = PolicyHelper.GetRetryAndCircuitBreakerHttpRequestExceptionOrOneOfResultWithNotFoundAsyncPolicySimple<TResult>();
        var handler = PolicyHelper.GetHandler<OneOf<TResult, NotFound, Error>>(logger);

        return new PolicyAndHandlerWrapper<OneOf<TResult, NotFound, Error>>(policy, handler);
    }

    public static PolicyAndHandlerWrapper<OneOf<TResult, NotFound, Error>> GetRetryAndCircuitBreakerHttpRequestExceptionOrOneOfResultWithNotFoundAsyncPolicyAndHandler<TResult>(RetryAndCircuitBreakerPolicyConfiguration configuration, ILogger logger)
    {
        var policy = PolicyHelper.GetRetryAndCircuitBreakerHttpRequestExceptionOrOneOfResultWithNotFoundAsyncPolicy<TResult>(configuration);
        var handler = PolicyHelper.GetHandler<OneOf<TResult, NotFound, Error>>(logger);

        return new PolicyAndHandlerWrapper<OneOf<TResult, NotFound, Error>>(policy, handler);
    }
}