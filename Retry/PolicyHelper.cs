using OneOf.Types;
using OneOf;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using System.Net;
using Polly.CircuitBreaker;

namespace Retry;

public static class PolicyHelper
{
    public static IAsyncPolicy<TResult> GetRetryAndCircuitBreakerExceptionAsyncPolicySimple<TResult, TException>()
        where TException : Exception
    {
        var retryPolicy = Policy<TResult>
            .Handle<TException>()
            .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(retryAttempt));
        var circuitBreakerPolicy = Policy<TResult>
            .Handle<TException>()
            .CircuitBreakerAsync(3, TimeSpan.FromSeconds(10));

        return circuitBreakerPolicy.WrapAsync(retryPolicy);
    }

    public static IAsyncPolicy<OneOf<TResult, NotFound, Error>> GetRetryAndCircuitBreakerOneOfResultWithNotFoundAsyncPolicySimple<TResult>() => 
        GetRetryAndCircuitBreakerResultAsyncPolicySimple<OneOf<TResult, NotFound, Error>>(static of => ShouldHandle(of));

    private static bool ShouldHandle<TResult>(OneOf<TResult, NotFound, Error> of)
    {
        if (of.IsT0 || of.IsT1)
        {
            return false;
        }

        return of.AsT2.StatusCode is >= HttpStatusCode.InternalServerError or HttpStatusCode.RequestTimeout;
    }

    public static IAsyncPolicy<TResult> GetRetryAndCircuitBreakerResultAsyncPolicySimple<TResult>(Func<TResult, bool> predicate)
    {
        var retryPolicy = Policy<TResult>
            .HandleResult(predicate)
            .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(retryAttempt));
        var circuitBreakerPolicy = Policy<TResult>
            .HandleResult(predicate)
            .CircuitBreakerAsync(3, TimeSpan.FromSeconds(10));

        return circuitBreakerPolicy.WrapAsync(retryPolicy);
    }

    public static IAsyncPolicy<OneOf<TResult, NotFound, Error>> GetRetryAndCircuitBreakerOneOfResultWithNotFoundAsyncPolicy<TResult>(RetryAndCircuitBreakerPolicyConfiguration configuration) =>
        GetRetryAndCircuitBreakerResultAsyncPolicy<OneOf<TResult, NotFound, Error>>(static of => ShouldHandle(of), configuration);

    public static IAsyncPolicy<TResult> GetRetryAndCircuitBreakerResultAsyncPolicy<TResult>(Func<TResult, bool> predicate, RetryAndCircuitBreakerPolicyConfiguration configuration)
    {
        var retryPolicy = Policy<TResult>
            .HandleResult(predicate)
            .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(configuration.FirstRetryDelay, configuration.RetryCount));
        var circuitBreakerPolicy = Policy<TResult>
            .HandleResult(predicate)
            .AdvancedCircuitBreakerAsync(configuration.FailureThreshold, configuration.SamplingDuration, configuration.MinimumThroughput, configuration.BreakDuration);

        return circuitBreakerPolicy.WrapAsync(retryPolicy);
    }

    public static IAsyncPolicy<HttpResponseMessage> GetRetryAndCircuitBreakerHttpRequestExceptionOrTransientHttpErrorAsyncPolicySimple()
    {
        var retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrTransientHttpError()
            .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(retryAttempt));
        var circuitBreakerPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrTransientHttpError()
            .CircuitBreakerAsync(3, TimeSpan.FromSeconds(10));

        return circuitBreakerPolicy.WrapAsync(retryPolicy);
    }

    public static IAsyncPolicy<TResult> GetRetryAndCircuitBreakerExceptionAsyncPolicy<TResult, TException>(RetryAndCircuitBreakerPolicyConfiguration configuration)
        where TException : Exception
    {
        var retryPolicy = Policy<TResult>
            .Handle<TException>()
            .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(configuration.FirstRetryDelay, configuration.RetryCount),
                OnRetry);
        var circuitBreakerPolicy = Policy<TResult>
            .Handle<TException>()
            .AdvancedCircuitBreakerAsync(configuration.FailureThreshold, configuration.SamplingDuration, configuration.MinimumThroughput, configuration.BreakDuration,
                OnBreak, OnReset, OnHalfOpen);

        return circuitBreakerPolicy.WrapAsync(retryPolicy);
    }

    private static void OnRetry<TResult>(DelegateResult<TResult> result, TimeSpan timeSpan, int count, Context context)
    {
    }

    private static void OnHalfOpen()
    {
    }

    private static void OnReset(Context context)
    {
    }

    private static void OnBreak<TResult>(DelegateResult<TResult> result, CircuitState state, TimeSpan timeSpan, Context context)
    {
    }
}