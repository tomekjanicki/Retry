using OneOf.Types;
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
        var builder = ResultHandleException<TResult, TException>();
        var retryPolicy = builder.ConfigureWaitAndRetryAsync();
        var circuitBreakerPolicy = builder.ConfigureCircuitBreakerAsync();

        return circuitBreakerPolicy.WrapAsync(retryPolicy);
    }

    public static IAsyncPolicy<OneOf<TResult, NotFound, Error>> GetRetryAndCircuitBreakerHttpRequestExceptionOrOneOfResultWithNotFoundAsyncPolicySimple<TResult>() =>
        GetRetryAndCircuitBreakerExceptionOrResultAsyncPolicySimple<OneOf<TResult, NotFound, Error>, HttpRequestException>(static of => ShouldHandle(of));

    public static IAsyncPolicy<TResult> GetRetryAndCircuitBreakerExceptionOrResultAsyncPolicySimple<TResult, TException>(Func<TResult, bool> predicate)
        where TException : Exception
    {
        var builder = ResultHandleExceptionOrResult<TResult, TException>(predicate);
        var retryPolicy = builder.ConfigureWaitAndRetryAsync();
        var circuitBreakerPolicy = builder.ConfigureCircuitBreakerAsync();

        return circuitBreakerPolicy.WrapAsync(retryPolicy);
    }

    public static IAsyncPolicy<OneOf<TResult, NotFound, Error>> GetRetryAndCircuitBreakerHttpRequestExceptionOrOneOfResultWithNotFoundAsyncPolicy<TResult>(RetryAndCircuitBreakerPolicyConfiguration configuration) =>
        GetRetryAndCircuitBreakerExceptionOrResultAsyncPolicy<OneOf<TResult, NotFound, Error>, HttpRequestException>(static of => ShouldHandle(of), configuration);

    public static IAsyncPolicy<TResult> GetRetryAndCircuitBreakerExceptionOrResultAsyncPolicy<TResult, TException>(Func<TResult, bool> predicate, RetryAndCircuitBreakerPolicyConfiguration configuration)
        where TException : Exception
    {
        var builder = ResultHandleExceptionOrResult<TResult, TException>(predicate);
        var retryPolicy = builder.ConfigureWaitAndRetryAsync(configuration.FirstRetryDelay, configuration.RetryCount);
        var circuitBreakerPolicy = builder.ConfigureAdvancedCircuitBreakerAsync(configuration.FailureThreshold, configuration.SamplingDuration, configuration.MinimumThroughput, configuration.BreakDuration);

        return circuitBreakerPolicy.WrapAsync(retryPolicy);
    }

    public static IAsyncPolicy<HttpResponseMessage> GetRetryAndCircuitBreakerHttpRequestExceptionOrTransientHttpErrorAsyncPolicySimple()
    {
        var builder = HttpResponseMessageHandleHttpRequestExceptionOrTransientHttpError();
        var retryPolicy = builder.ConfigureWaitAndRetryAsync();
        var circuitBreakerPolicy = builder.ConfigureCircuitBreakerAsync();

        return circuitBreakerPolicy.WrapAsync(retryPolicy);
    }

    public static IAsyncPolicy<TResult> GetRetryAndCircuitBreakerExceptionAsyncPolicy<TResult, TException>(RetryAndCircuitBreakerPolicyConfiguration configuration)
        where TException : Exception
    {
        var builder = ResultHandleException<TResult, TException>();
        var retryPolicy = builder.ConfigureWaitAndRetryAsync(configuration.FirstRetryDelay, configuration.RetryCount);
        var circuitBreakerPolicy = builder.ConfigureAdvancedCircuitBreakerAsync(configuration.FailureThreshold, configuration.SamplingDuration, configuration.MinimumThroughput, configuration.BreakDuration);

        return circuitBreakerPolicy.WrapAsync(retryPolicy);
    }

    private static PolicyBuilder<TResult> ResultHandleException<TResult, TException>()
        where TException : Exception =>
        Policy<TResult>.Handle<TException>();

    private static PolicyBuilder<TResult> ResultHandleExceptionOrResult<TResult, TException>(Func<TResult, bool> predicate)
        where TException : Exception =>
        Policy<TResult>.Handle<TException>().OrResult(predicate);

    private static PolicyBuilder<HttpResponseMessage> HttpResponseMessageHandleHttpRequestExceptionOrTransientHttpError() =>
        Policy<HttpResponseMessage>.Handle<HttpRequestException>().OrTransientHttpError();

    private static AsyncRetryPolicy<TResult> ConfigureWaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder) =>
        policyBuilder.WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(retryAttempt), OnRetry);

    private static AsyncRetryPolicy<TResult> ConfigureWaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, TimeSpan firstRetryDelay, int retryCount) => 
        policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(firstRetryDelay, retryCount), OnRetry);

    private static AsyncCircuitBreakerPolicy<TResult> ConfigureCircuitBreakerAsync<TResult>(this PolicyBuilder<TResult> policyBuilder) =>
        policyBuilder.CircuitBreakerAsync(3, TimeSpan.FromSeconds(10), OnBreak, _ => { }, () => {});

    private static AsyncCircuitBreakerPolicy<TResult> ConfigureAdvancedCircuitBreakerAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, double failureThreshold,
        TimeSpan samplingDuration, int minimumThroughput, TimeSpan breakDuration) =>
        policyBuilder.AdvancedCircuitBreakerAsync(failureThreshold, samplingDuration, minimumThroughput, breakDuration, OnBreak, _ => { }, () => { });

    private static bool ShouldHandle<TResult>(OneOf<TResult, NotFound, Error> of) =>
        of is { IsT0: false, IsT1: false } && of.AsT2.StatusCode is >= HttpStatusCode.InternalServerError or HttpStatusCode.RequestTimeout;

    private static void OnRetry<TResult>(DelegateResult<TResult> result, TimeSpan timeSpan, int count, Context context)
    {
        var value = context.TryGetValue<Handlers<TResult>>(Handlers);
        value?.OnRetry(result, value.Logger);
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