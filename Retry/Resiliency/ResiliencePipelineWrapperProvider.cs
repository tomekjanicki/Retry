using Microsoft.Extensions.Options;
using OneOf;
using OneOf.Types;
using Retry.Extensions;
using Retry.Resiliency.Models;

namespace Retry.Resiliency;

public sealed class ResiliencePipelineWrapperProvider
{
    private readonly RetryAndCircuitBreakerPolicyConfiguration _configuration;

    public ResiliencePipelineWrapperProvider(IOptions<ConfigurationSettings> options) =>
        _configuration = options.Value.RetryAndCircuitBreakerPolicyConfiguration;

    public ResiliencePipelineWrapper<TResult> GetRetryAndCircuitBreakerTransientHttpRequestExceptionPipelineWrapper<TResult>(ILogger logger) =>
        GetRetryAndCircuitBreakerExceptionPipelineWrapper<TResult, HttpRequestException>(logger, static exception => exception.ShouldHandleTransientHttpRequestException());

    public ResiliencePipelineWrapper<TResult> GetRetryAndCircuitBreakerExceptionPipelineWrapper<TResult,
        TException>(ILogger logger, Func<TException, bool> predicate)
        where TException : Exception
    {
        var pipeline = GenericResiliencePipelines.GetResultHandleExceptionRetryAndCircuitBreaker<TResult, TException>(_configuration, predicate);

        return new ResiliencePipelineWrapper<TResult>(pipeline, logger);
    }

    public ResiliencePipelineWrapper GetRetryAndCircuitBreakerExceptionPipelineWrapper<TException>(ILogger logger, Func<TException, bool> predicate)
        where TException : Exception
    {
        var pipeline = GenericResiliencePipelines.GetHandleExceptionRetryAndCircuitBreaker(_configuration, predicate);

        return new ResiliencePipelineWrapper(pipeline, logger);
    }

    public ResiliencePipelineWrapper<TResult> GetCircuitBreakerExceptionPipelineWrapper<TResult,
        TException>(ILogger logger, Func<TException, bool> predicate)
        where TException : Exception
    {
        var pipeline = GenericResiliencePipelines.GetResultHandleExceptionCircuitBreaker<TResult, TException>(_configuration.CircuitBreakerPolicy, predicate);

        return new ResiliencePipelineWrapper<TResult>(pipeline, logger);
    }

    public ResiliencePipelineWrapper GetCircuitBreakerExceptionPipelineWrapper<TException>(ILogger logger, Func<TException, bool> predicate)
        where TException : Exception
    {
        var pipeline = GenericResiliencePipelines.GetHandleExceptionCircuitBreaker(_configuration.CircuitBreakerPolicy, predicate);

        return new ResiliencePipelineWrapper(pipeline, logger);
    }

    public ResiliencePipelineWrapper<OneOf<TResult, NotFound, ApiError>> GetRetryAndCircuitBreakerOneOfResultWithNotFoundPipelineWrapper<TResult>(ILogger logger)
    {
        var pipeline = ExtendedResiliencePipelines.GetOneOfResultWithNotFoundRetryAndCircuitBreaker<TResult>(_configuration);

        return new ResiliencePipelineWrapper<OneOf<TResult, NotFound, ApiError>>(pipeline, logger);
    }
}