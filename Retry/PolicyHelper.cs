﻿using OneOf.Types;
using OneOf;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using System.Net;
using Polly.CircuitBreaker;
using Polly.Retry;
using Retry.Extensions;

namespace Retry;

public static class PolicyHelper
{
    public static Handlers<TResult> GetHandler<TResult>(ILogger logger) => new(logger, GetBreakLogResult<TResult>(), GetRetryLogResult<TResult>());

    public static Action<DelegateResult<TResult>, ILogger> GetRetryLogResult<TResult>() => GetLogResult<TResult>(true);

    public static Action<DelegateResult<TResult>, ILogger> GetBreakLogResult<TResult>() => GetLogResult<TResult>(false);

    public static IAsyncPolicy<TResult> GetRetryAndCircuitBreakerExceptionAsyncPolicySimple<TResult, TException>()
        where TException : Exception
    {
        var retryPolicy = Policy<TResult>
            .Handle<TException>()
            .ConfigureWaitAndRetryAsync();
        var circuitBreakerPolicy = Policy<TResult>
            .Handle<TException>()
            .ConfigureCircuitBreakerAsync();

        return circuitBreakerPolicy.WrapAsync(retryPolicy);
    }

    public static IAsyncPolicy<OneOf<TResult, NotFound, Error>> GetRetryAndCircuitBreakerHttpRequestExceptionOrOneOfResultWithNotFoundAsyncPolicySimple<TResult>() =>
        GetRetryAndCircuitBreakerExceptionOrResultAsyncPolicySimple<OneOf<TResult, NotFound, Error>, HttpRequestException>(static of => ShouldHandle(of));

    public static IAsyncPolicy<TResult> GetRetryAndCircuitBreakerExceptionOrResultAsyncPolicySimple<TResult, TException>(Func<TResult, bool> predicate)
        where TException : Exception
    {
        var retryPolicy = Policy<TResult>
            .Handle<TException>()
            .OrResult(predicate)
            .ConfigureWaitAndRetryAsync();
        var circuitBreakerPolicy = Policy<TResult>
            .Handle<TException>()
            .OrResult(predicate)
            .ConfigureCircuitBreakerAsync();

        return circuitBreakerPolicy.WrapAsync(retryPolicy);
    }

    public static IAsyncPolicy<OneOf<TResult, NotFound, Error>> GetRetryAndCircuitBreakerHttpRequestExceptionOrOneOfResultWithNotFoundAsyncPolicy<TResult>(RetryAndCircuitBreakerPolicyConfiguration configuration) =>
        GetRetryAndCircuitBreakerExceptionOrResultAsyncPolicy<OneOf<TResult, NotFound, Error>, HttpRequestException>(static of => ShouldHandle(of), configuration);

    public static IAsyncPolicy<TResult> GetRetryAndCircuitBreakerExceptionOrResultAsyncPolicy<TResult, TException>(Func<TResult, bool> predicate, RetryAndCircuitBreakerPolicyConfiguration configuration)
        where TException : Exception
    {
        var retryPolicy = Policy<TResult>
            .Handle<TException>()
            .OrResult(predicate)
            .ConfigureWaitAndRetryAsync(configuration.FirstRetryDelay, configuration.RetryCount);
        var circuitBreakerPolicy = Policy<TResult>
            .Handle<TException>()
            .OrResult(predicate)
            .ConfigureAdvancedCircuitBreakerAsync(configuration.FailureThreshold, configuration.SamplingDuration, configuration.MinimumThroughput, configuration.BreakDuration);

        return circuitBreakerPolicy.WrapAsync(retryPolicy);
    }

    public static IAsyncPolicy<HttpResponseMessage> GetRetryAndCircuitBreakerHttpRequestExceptionOrTransientHttpErrorAsyncPolicySimple()
    {
        var retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrTransientHttpError()
            .ConfigureWaitAndRetryAsync();
        var circuitBreakerPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrTransientHttpError()
            .ConfigureCircuitBreakerAsync();

        return circuitBreakerPolicy.WrapAsync(retryPolicy);
    }

    public static IAsyncPolicy<TResult> GetRetryAndCircuitBreakerExceptionAsyncPolicy<TResult, TException>(RetryAndCircuitBreakerPolicyConfiguration configuration)
        where TException : Exception
    {
        var retryPolicy = Policy<TResult>
            .Handle<TException>()
            .ConfigureWaitAndRetryAsync(configuration.FirstRetryDelay, configuration.RetryCount);
        var circuitBreakerPolicy = Policy<TResult>
            .Handle<TException>()
            .ConfigureAdvancedCircuitBreakerAsync(configuration.FailureThreshold, configuration.SamplingDuration, configuration.MinimumThroughput, configuration.BreakDuration);

        return circuitBreakerPolicy.WrapAsync(retryPolicy);
    }

    private static AsyncRetryPolicy<TResult> ConfigureWaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder) =>
        policyBuilder.WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(retryAttempt), OnRetry);

    private static AsyncRetryPolicy<TResult> ConfigureWaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, TimeSpan firstRetryDelay, int retryCount) => 
        policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(firstRetryDelay, retryCount), OnRetry);

    private static AsyncCircuitBreakerPolicy<TResult> ConfigureCircuitBreakerAsync<TResult>(this PolicyBuilder<TResult> policyBuilder) =>
        policyBuilder.CircuitBreakerAsync(3, TimeSpan.FromSeconds(10), OnBreak, OnReset, OnHalfOpen);

    private static AsyncCircuitBreakerPolicy<TResult> ConfigureAdvancedCircuitBreakerAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, double failureThreshold,
        TimeSpan samplingDuration, int minimumThroughput, TimeSpan breakDuration) =>
        policyBuilder.AdvancedCircuitBreakerAsync(failureThreshold, samplingDuration, minimumThroughput, breakDuration, OnBreak, OnReset, OnHalfOpen);

    private static bool ShouldHandle<TResult>(OneOf<TResult, NotFound, Error> of)
    {
        if (of.IsT0 || of.IsT1)
        {
            return false;
        }

        return of.AsT2.StatusCode is >= HttpStatusCode.InternalServerError or HttpStatusCode.RequestTimeout;
    }

    private static void OnRetry<TResult>(DelegateResult<TResult> result, TimeSpan timeSpan, int count, Context context)
    {
        var value = context.TryGetValue<Handlers<TResult>>(Handlers);
        value?.OnRetry(result, value.Logger);
    }

    private static void OnHalfOpen()
    {
    }
    private static void OnReset(Context context)
    {
    }

    private static void OnBreak<TResult>(DelegateResult<TResult> result, CircuitState state, TimeSpan timeSpan, Context context)
    {
        var value = context.TryGetValue<Handlers<TResult>>(Handlers);
        value?.OnBrake(result, value.Logger);
    }

    private static Action<DelegateResult<TResult>, ILogger> GetLogResult<TResult>(bool retry) => 
        retry ? static (result, logger) => logger.RetryLogResult(result) : static (result, logger) => logger.BreakLogResult(result);

    private static void RetryLogResult<TResult>(this ILogger logger, DelegateResult<TResult> result)
    {
        if (result.Exception is not null)
        {
            logger.LogError(result.Exception, "Exception during retry.");

            return;
        }

        logger.LogError("Error during retry. {Result}", result.Result);
    }

    private static void BreakLogResult<TResult>(this ILogger logger, DelegateResult<TResult> result)
    {
        if (result.Exception is not null)
        {
            logger.LogError(result.Exception, "Exception during break.");

            return;
        }

        logger.LogError("Error during break. {Result}", result.Result);
    }

    public const string Handlers = "handlers";
}