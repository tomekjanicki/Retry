using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Retry.Extensions;
using Retry.Resiliency.Models;

namespace Retry.Resiliency;

public static class GenericResiliencePipelines
{
    public static readonly ResiliencePropertyKey<ILogger> Key = new("logger");

    public static ResiliencePipeline<TResult> HandleResultRetryAndCircuitBreaker<TResult>(RetryAndCircuitBreakerPolicyConfiguration configuration,
        Func<RetryPredicateArguments<TResult>, ValueTask<bool>> retryShouldHandle, Func<CircuitBreakerPredicateArguments<TResult>, ValueTask<bool>> circuitBreakerShouldHandle) =>
        new ResiliencePipelineBuilder<TResult>()
            .AddRetry(GetResultRetryStrategyOptions(configuration.RetryPolicy, retryShouldHandle))
            .AddCircuitBreaker(GetResultCircuitBreakerStrategyOptions(configuration.CircuitBreakerPolicy, circuitBreakerShouldHandle))
            .Build();

    public static ResiliencePipeline HandleNoResultRetryAndCircuitBreaker(RetryAndCircuitBreakerPolicyConfiguration configuration,
        Func<RetryPredicateArguments<object>, ValueTask<bool>> retryShouldHandle, Func<CircuitBreakerPredicateArguments<object>, ValueTask<bool>> circuitBreakerShouldHandle) =>
        new ResiliencePipelineBuilder()
            .AddRetry(GetRetryStrategyOptions(configuration.RetryPolicy, retryShouldHandle))
            .AddCircuitBreaker(GetCircuitBreakerStrategyOptions(configuration.CircuitBreakerPolicy, circuitBreakerShouldHandle))
            .Build();

    public static ResiliencePipeline<TResult> HandleResultCircuitBreaker<TResult>(CircuitBreakerPolicyConfiguration configuration,
        Func<CircuitBreakerPredicateArguments<TResult>, ValueTask<bool>> circuitBreakerShouldHandle) =>
        new ResiliencePipelineBuilder<TResult>()
            .AddCircuitBreaker(GetResultCircuitBreakerStrategyOptions(configuration, circuitBreakerShouldHandle))
            .Build();

    public static ResiliencePipeline HandleNoResultCircuitBreaker(CircuitBreakerPolicyConfiguration configuration,
        Func<CircuitBreakerPredicateArguments<object>, ValueTask<bool>> circuitBreakerShouldHandle) =>
        new ResiliencePipelineBuilder()
            .AddCircuitBreaker(GetCircuitBreakerStrategyOptions(configuration, circuitBreakerShouldHandle))
            .Build();

    private static RetryStrategyOptions<TResult> GetResultRetryStrategyOptions<TResult>(RetryPolicyConfiguration configuration,
        Func<RetryPredicateArguments<TResult>, ValueTask<bool>> shouldHandle) =>
        new()
        {
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            Delay = configuration.FirstRetryDelay,
            MaxRetryAttempts = configuration.RetryCount,
            ShouldHandle = shouldHandle, 
            OnRetry = static arguments => OnRetry(arguments)
        };

    private static RetryStrategyOptions GetRetryStrategyOptions(RetryPolicyConfiguration configuration,
        Func<RetryPredicateArguments<object>, ValueTask<bool>> shouldHandle) =>
        new()
        {
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            Delay = configuration.FirstRetryDelay,
            MaxRetryAttempts = configuration.RetryCount,
            ShouldHandle = shouldHandle,
            OnRetry = static arguments => OnRetry(arguments)
        };

    private static CircuitBreakerStrategyOptions GetCircuitBreakerStrategyOptions(CircuitBreakerPolicyConfiguration configuration,
        Func<CircuitBreakerPredicateArguments<object>, ValueTask<bool>> shouldHandle) =>
        new()
        {
            ShouldHandle = shouldHandle,
            BreakDuration = configuration.BreakDuration,
            MinimumThroughput = configuration.MinimumThroughput,
            FailureRatio = configuration.FailureThreshold,
            SamplingDuration = configuration.SamplingDuration,
            OnOpened = static arguments => OnOpen(arguments)
        };

    private static CircuitBreakerStrategyOptions<TResult> GetResultCircuitBreakerStrategyOptions<TResult>(CircuitBreakerPolicyConfiguration configuration,
        Func<CircuitBreakerPredicateArguments<TResult>, ValueTask<bool>> shouldHandle) =>
        new()
        {
            ShouldHandle = shouldHandle,
            BreakDuration = configuration.BreakDuration,
            MinimumThroughput = configuration.MinimumThroughput,
            FailureRatio = configuration.FailureThreshold,
            SamplingDuration = configuration.SamplingDuration,
            OnOpened = static arguments => OnOpen(arguments)
        };

    private static ValueTask OnOpen<TResult>(OnCircuitOpenedArguments<TResult> arguments) => 
        On(arguments.Context, arguments.Outcome, static (logger, outcome) => logger.OpenLogResult(outcome));

    private static ValueTask On<TResult>(ResilienceContext resilienceContext, Outcome<TResult> outcome, Action<ILogger, Outcome<TResult>> logAction)
    {
        var tryGetValue = resilienceContext.Properties.TryGetValue(Key, out var logger);
        if (!tryGetValue || logger is null)
        {
            return ValueTask.CompletedTask;
        }

        logAction(logger, outcome);

        return ValueTask.CompletedTask;
    }

    private static ValueTask OnRetry<TResult>(OnRetryArguments<TResult> arguments) =>
        On(arguments.Context, arguments.Outcome, static (logger, outcome) => logger.RetryLogResult(outcome));
}