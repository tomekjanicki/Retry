using Polly;
using Polly.CircuitBreaker;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using Retry.Resiliency.Models;

namespace Retry.Extensions;

public static class PolicyBuilderExtensions
{
    public const string Handlers = "handlers";

    public static IAsyncPolicy GetCircuitBreakerAsyncPolicy(this PolicyBuilder builder, CircuitBreakerPolicyConfiguration configuration) =>
        builder.ConfigureAdvancedCircuitBreakerAsync(configuration.FailureThreshold, configuration.SamplingDuration, configuration.MinimumThroughput, configuration.BreakDuration);

    public static IAsyncPolicy<TResult> GetCircuitBreakerAsyncPolicy<TResult>(this PolicyBuilder<TResult> builder, CircuitBreakerPolicyConfiguration configuration) =>
        builder.ConfigureAdvancedCircuitBreakerAsync(configuration.FailureThreshold, configuration.SamplingDuration, configuration.MinimumThroughput, configuration.BreakDuration);

    public static IAsyncPolicy<TResult> GetRetryAndCircuitBreakerAsyncPolicy<TResult>(this PolicyBuilder<TResult> builder,
        RetryAndCircuitBreakerPolicyConfiguration configuration)
    {
        var retryPolicy = builder.ConfigureWaitAndRetryAsync(configuration.RetryPolicy.FirstRetryDelay, configuration.RetryPolicy.RetryCount);
        var circuitBreakerPolicy = builder.ConfigureAdvancedCircuitBreakerAsync(configuration.CircuitBreakerPolicy.FailureThreshold,
            configuration.CircuitBreakerPolicy.SamplingDuration, configuration.CircuitBreakerPolicy.MinimumThroughput, configuration.CircuitBreakerPolicy.BreakDuration);

        return circuitBreakerPolicy.WrapAsync(retryPolicy);
    }

    public static IAsyncPolicy GetRetryAndCircuitBreakerAsyncPolicy(this PolicyBuilder builder, RetryAndCircuitBreakerPolicyConfiguration configuration)
    {
        var retryPolicy = builder.ConfigureWaitAndRetryAsync(configuration.RetryPolicy.FirstRetryDelay, configuration.RetryPolicy.RetryCount);
        var circuitBreakerPolicy = builder.ConfigureAdvancedCircuitBreakerAsync(configuration.CircuitBreakerPolicy.FailureThreshold,
            configuration.CircuitBreakerPolicy.SamplingDuration, configuration.CircuitBreakerPolicy.MinimumThroughput, configuration.CircuitBreakerPolicy.BreakDuration);

        return circuitBreakerPolicy.WrapAsync(retryPolicy);
    }

    public static IAsyncPolicy<TResult> GetRetryAndCircuitBreakerAsyncPolicy<TResult>(this PolicyBuilder<TResult> builder)
    {
        var retryPolicy = builder.ConfigureWaitAndRetryAsync();
        var circuitBreakerPolicy = builder.ConfigureCircuitBreakerAsync();

        return circuitBreakerPolicy.WrapAsync(retryPolicy);
    }

    public static IAsyncPolicy GetRetryAndCircuitBreakerAsyncPolicy(this PolicyBuilder builder)
    {
        var retryPolicy = builder.ConfigureWaitAndRetryAsync();
        var circuitBreakerPolicy = builder.ConfigureCircuitBreakerAsync();

        return circuitBreakerPolicy.WrapAsync(retryPolicy);
    }

    public static IAsyncPolicy<TResult> GetCircuitBreakerAsyncPolicy<TResult>(this PolicyBuilder<TResult> builder) => 
        builder.ConfigureCircuitBreakerAsync();

    public static IAsyncPolicy GetCircuitBreakerAsyncPolicy(this PolicyBuilder builder) =>
        builder.ConfigureCircuitBreakerAsync();

    public static AsyncRetryPolicy<TResult> ConfigureWaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder) =>
        policyBuilder.WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(retryAttempt), OnRetry);

    public static AsyncRetryPolicy<TResult> ConfigureWaitAndRetryAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, TimeSpan firstRetryDelay, int retryCount) =>
        policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(firstRetryDelay, retryCount), OnRetry);

    public static AsyncRetryPolicy ConfigureWaitAndRetryAsync(this PolicyBuilder policyBuilder) =>
        policyBuilder.WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(retryAttempt), OnRetry);

    public static AsyncRetryPolicy ConfigureWaitAndRetryAsync(this PolicyBuilder policyBuilder, TimeSpan firstRetryDelay, int retryCount) =>
        policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(firstRetryDelay, retryCount), OnRetry);

    public static AsyncCircuitBreakerPolicy<TResult> ConfigureCircuitBreakerAsync<TResult>(this PolicyBuilder<TResult> policyBuilder) =>
        policyBuilder.CircuitBreakerAsync(3, TimeSpan.FromSeconds(10), OnBreak, _ => { }, () => { });

    public static AsyncCircuitBreakerPolicy ConfigureCircuitBreakerAsync(this PolicyBuilder policyBuilder) =>
        policyBuilder.CircuitBreakerAsync(3, TimeSpan.FromSeconds(10), OnBreak, _ => { }, () => { });

    public static AsyncCircuitBreakerPolicy<TResult> ConfigureAdvancedCircuitBreakerAsync<TResult>(this PolicyBuilder<TResult> policyBuilder, double failureThreshold,
        TimeSpan samplingDuration, int minimumThroughput, TimeSpan breakDuration) =>
        policyBuilder.AdvancedCircuitBreakerAsync(failureThreshold, samplingDuration, minimumThroughput, breakDuration, OnBreak, _ => { }, () => { });

    public static AsyncCircuitBreakerPolicy ConfigureAdvancedCircuitBreakerAsync(this PolicyBuilder policyBuilder, double failureThreshold,
        TimeSpan samplingDuration, int minimumThroughput, TimeSpan breakDuration) =>
        policyBuilder.AdvancedCircuitBreakerAsync(failureThreshold, samplingDuration, minimumThroughput, breakDuration, OnBreak, _ => { }, () => { });

    private static void OnRetry<TResult>(DelegateResult<TResult> result, TimeSpan timeSpan, int count, Context context)
    {
        var value = context.TryGetValue<Handlers<TResult>>(Handlers);
        value?.OnRetry(result, value.Logger);
    }

    private static void OnRetry(Exception exception, TimeSpan timeSpan, int count, Context context)
    {
        var value = context.TryGetValue<Handlers>(Handlers);
        value?.OnRetry(exception, value.Logger);
    }

    private static void OnBreak<TResult>(DelegateResult<TResult> result, CircuitState state, TimeSpan timeSpan, Context context)
    {
        var value = context.TryGetValue<Handlers<TResult>>(Handlers);
        value?.OnBrake(result, value.Logger);
    }

    private static void OnBreak(Exception exception, CircuitState state, TimeSpan timeSpan, Context context)
    {
        var value = context.TryGetValue<Handlers>(Handlers);
        value?.OnBrake(exception, value.Logger);
    }
}