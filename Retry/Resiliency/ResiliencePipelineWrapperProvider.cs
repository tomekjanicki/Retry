using Microsoft.Extensions.Options;
using OneOf;
using OneOf.Types;
using Polly.CircuitBreaker;
using Polly.Retry;
using Retry.Extensions;
using Retry.Resiliency.Models;

namespace Retry.Resiliency;

public sealed class ResiliencePipelineWrapperProvider
{
    private readonly RetryAndCircuitBreakerPolicyConfiguration _configuration;

    public ResiliencePipelineWrapperProvider(IOptions<ConfigurationSettings> options) =>
        _configuration = options.Value.RetryAndCircuitBreakerPolicyConfiguration;

    public ResiliencePipelineWrapper<TResult> HandleResultTransientHttpRequestExceptionRetryAndCircuitBreakerPipelineWrapper<TResult>(ILogger logger) =>
        HandleResultRetryAndCircuitBreakerPipelineWrapper<TResult>(logger,
            static arguments => arguments.Outcome.ShouldHandleException<TResult, HttpRequestException>(static exception => exception.ShouldHandleTransientHttpRequestException()),
            static arguments => arguments.Outcome.ShouldHandleException<TResult, HttpRequestException>(static exception => exception.ShouldHandleTransientHttpRequestException()));

    public ResiliencePipelineWrapper<TResult> HandleResultRetryAndCircuitBreakerPipelineWrapper<TResult>(ILogger logger, Func<RetryPredicateArguments<TResult>, ValueTask<bool>> retryShouldHandle, Func<CircuitBreakerPredicateArguments<TResult>, ValueTask<bool>> circuitBreakerShouldHandle)
    {
        var pipeline = GenericResiliencePipelines.HandleResultRetryAndCircuitBreaker(_configuration, retryShouldHandle, circuitBreakerShouldHandle);

        return new ResiliencePipelineWrapper<TResult>(pipeline, logger);
    }

    public ResiliencePipelineWrapper HandleNoResultRetryAndCircuitBreakerPipelineWrapper(ILogger logger, Func<RetryPredicateArguments<object>, ValueTask<bool>> retryShouldHandle, Func<CircuitBreakerPredicateArguments<object>, ValueTask<bool>> circuitBreakerShouldHandle)
    {
        var pipeline = GenericResiliencePipelines.HandleNoResultRetryAndCircuitBreaker(_configuration, retryShouldHandle, circuitBreakerShouldHandle);

        return new ResiliencePipelineWrapper(pipeline, logger);
    }

    public ResiliencePipelineWrapper<TResult> HandleResultCircuitBreakerPipelineWrapper<TResult>(ILogger logger, Func<CircuitBreakerPredicateArguments<TResult>, ValueTask<bool>> circuitBreakerShouldHandle)
    {
        var pipeline = GenericResiliencePipelines.HandleResultCircuitBreaker(_configuration.CircuitBreakerPolicy, circuitBreakerShouldHandle);

        return new ResiliencePipelineWrapper<TResult>(pipeline, logger);
    }

    public ResiliencePipelineWrapper HandleNoResultCircuitBreakerPipelineWrapper(ILogger logger, Func<CircuitBreakerPredicateArguments<object>, ValueTask<bool>> circuitBreakerShouldHandle)
    {
        var pipeline = GenericResiliencePipelines.HandleNoResultCircuitBreaker(_configuration.CircuitBreakerPolicy, circuitBreakerShouldHandle);

        return new ResiliencePipelineWrapper(pipeline, logger);
    }

    public ResiliencePipelineWrapper<OneOf<TResult, NotFound, ApiError>> GetRetryAndCircuitBreakerOneOfResultWithNotFoundPipelineWrapper<TResult>(ILogger logger)
    {
        var pipeline = ExtendedResiliencePipelines.HandleOneOfResultWithNotFoundRetryAndCircuitBreaker<TResult>(_configuration);

        return new ResiliencePipelineWrapper<OneOf<TResult, NotFound, ApiError>>(pipeline, logger);
    }
}