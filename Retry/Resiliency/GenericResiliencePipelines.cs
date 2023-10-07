using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Retry.Extensions;
using Retry.Resiliency.Models;

namespace Retry.Resiliency;

public static class GenericResiliencePipelines
{
    public static readonly ResiliencePropertyKey<ILogger> Key = new("logger");

    public static ResiliencePipeline<TResult> GetResultHandleExceptionOrResultRetryAndCircuitBreaker<TResult, TException>(RetryAndCircuitBreakerPolicyConfiguration configuration,
        Func<TResult, bool> resultPredicate, Func<TException, bool> exceptionPredicate)
        where TException : Exception =>
        new ResiliencePipelineBuilder<TResult>()
            .AddRetry(GetResultRetryStrategyOptions(configuration.RetryPolicy, ResultHandleExceptionOrResult(resultPredicate, exceptionPredicate)))
            .AddCircuitBreaker(GetResultCircuitBreakerStrategyOptions(configuration.CircuitBreakerPolicy, ResultHandleExceptionOrResult(resultPredicate, exceptionPredicate)))
            .Build();

    public static ResiliencePipeline<TResult> GetResultHandleResultRetryAndCircuitBreaker<TResult>(RetryAndCircuitBreakerPolicyConfiguration configuration,
        Func<TResult, bool> resultPredicate) =>
        new ResiliencePipelineBuilder<TResult>()
            .AddRetry(GetResultRetryStrategyOptions(configuration.RetryPolicy, ResultHandleResult(resultPredicate)))
            .AddCircuitBreaker(GetResultCircuitBreakerStrategyOptions(configuration.CircuitBreakerPolicy, ResultHandleResult(resultPredicate)))
            .Build();

    public static ResiliencePipeline<TResult> GetResultHandleExceptionRetryAndCircuitBreaker<TResult, TException>(RetryAndCircuitBreakerPolicyConfiguration configuration,
        Func<TException, bool> exceptionPredicate)
        where TException : Exception =>
        new ResiliencePipelineBuilder<TResult>()
            .AddRetry(GetResultRetryStrategyOptions(configuration.RetryPolicy, ResultHandleException<TResult, TException>(exceptionPredicate)))
            .AddCircuitBreaker(GetResultCircuitBreakerStrategyOptions(configuration.CircuitBreakerPolicy, ResultHandleException<TResult, TException>(exceptionPredicate)))
            .Build();

    public static ResiliencePipeline GetHandleExceptionRetryAndCircuitBreaker<TException>(RetryAndCircuitBreakerPolicyConfiguration configuration,
        Func<TException, bool> exceptionPredicate)
        where TException : Exception =>
        new ResiliencePipelineBuilder()
            .AddRetry(GetRetryStrategyOptions(configuration.RetryPolicy, HandleException(exceptionPredicate)))
            .AddCircuitBreaker(GetCircuitBreakerStrategyOptions(configuration.CircuitBreakerPolicy, HandleException(exceptionPredicate)))
            .Build();

    public static ResiliencePipeline<TResult> GetResultHandleExceptionOrResultCircuitBreaker<TResult, TException>(CircuitBreakerPolicyConfiguration configuration,
        Func<TResult, bool> resultPredicate, Func<TException, bool> exceptionPredicate)
        where TException : Exception =>
        new ResiliencePipelineBuilder<TResult>()
            .AddCircuitBreaker(GetResultCircuitBreakerStrategyOptions(configuration, ResultHandleExceptionOrResult(resultPredicate, exceptionPredicate)))
            .Build();

    public static ResiliencePipeline<TResult> GetResultHandleResultCircuitBreaker<TResult>(RetryAndCircuitBreakerPolicyConfiguration configuration,
        Func<TResult, bool> resultPredicate) =>
        new ResiliencePipelineBuilder<TResult>()
            .AddCircuitBreaker(GetResultCircuitBreakerStrategyOptions(configuration.CircuitBreakerPolicy, ResultHandleResult(resultPredicate)))
            .Build();

    public static ResiliencePipeline<TResult> GetResultHandleExceptionCircuitBreaker<TResult, TException>(CircuitBreakerPolicyConfiguration configuration,
        Func<TException, bool> exceptionPredicate)
        where TException : Exception =>
        new ResiliencePipelineBuilder<TResult>()
            .AddCircuitBreaker(GetResultCircuitBreakerStrategyOptions(configuration, ResultHandleException<TResult, TException>(exceptionPredicate)))
            .Build();

    public static ResiliencePipeline GetHandleExceptionCircuitBreaker<TException>(CircuitBreakerPolicyConfiguration configuration,
        Func<TException, bool> exceptionPredicate)
        where TException : Exception =>
        new ResiliencePipelineBuilder()
            .AddCircuitBreaker(GetCircuitBreakerStrategyOptions(configuration, HandleException(exceptionPredicate)))
            .Build();

    private static RetryStrategyOptions<TResult> GetResultRetryStrategyOptions<TResult>(RetryPolicyConfiguration configuration,
        PredicateBuilder<TResult> predicateBuilder) =>
        new()
        {
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            Delay = configuration.FirstRetryDelay,
            MaxRetryAttempts = configuration.RetryCount,
            ShouldHandle = predicateBuilder,
            OnRetry = static arguments => OnRetry(arguments)
        };

    private static RetryStrategyOptions GetRetryStrategyOptions(RetryPolicyConfiguration configuration,
        PredicateBuilder<object> predicateBuilder) =>
        new()
        {
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            Delay = configuration.FirstRetryDelay,
            MaxRetryAttempts = configuration.RetryCount,
            ShouldHandle = predicateBuilder,
            OnRetry = static arguments => OnRetry(arguments)
        };

    private static CircuitBreakerStrategyOptions GetCircuitBreakerStrategyOptions(CircuitBreakerPolicyConfiguration configuration,
        PredicateBuilder<object> predicateBuilder) =>
        new()
        {
            ShouldHandle = predicateBuilder,
            BreakDuration = configuration.BreakDuration,
            MinimumThroughput = configuration.MinimumThroughput,
            FailureRatio = configuration.FailureThreshold,
            SamplingDuration = configuration.SamplingDuration,
            OnOpened = static arguments => OnOpen(arguments)
        };

    private static CircuitBreakerStrategyOptions<TResult> GetResultCircuitBreakerStrategyOptions<TResult>(CircuitBreakerPolicyConfiguration configuration,
        PredicateBuilder<TResult> predicateBuilder) =>
        new()
        {
            ShouldHandle = predicateBuilder,
            BreakDuration = configuration.BreakDuration,
            MinimumThroughput = configuration.MinimumThroughput,
            FailureRatio = configuration.FailureThreshold,
            SamplingDuration = configuration.SamplingDuration,
            OnOpened = static arguments => OnOpen(arguments)
        };

    private static PredicateBuilder<TResult> ResultHandleExceptionOrResult<TResult, TException>(Func<TResult, bool> resultPredicate, Func<TException, bool> exceptionPredicate)
        where TException : Exception =>
        new PredicateBuilder<TResult>()
            .Handle(exceptionPredicate)
            .HandleResult(resultPredicate);

    private static PredicateBuilder<TResult> ResultHandleResult<TResult>(Func<TResult, bool> resultPredicate) =>
        new PredicateBuilder<TResult>()
            .HandleResult(resultPredicate);

    private static PredicateBuilder<TResult> ResultHandleException<TResult, TException>(Func<TException, bool> exceptionPredicate)
        where TException : Exception =>
        new PredicateBuilder<TResult>()
            .Handle(exceptionPredicate);

    private static PredicateBuilder<object> HandleException<TException>(Func<TException, bool> exceptionPredicate)
        where TException : Exception =>
        new PredicateBuilder()
            .Handle(exceptionPredicate);

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