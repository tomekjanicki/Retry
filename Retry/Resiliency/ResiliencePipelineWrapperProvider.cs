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

    public ResiliencePipelineWrapper<TResult> GetRetryAndCircuitBreakerTransientHttpRequestExceptionAsyncPolicyAndHandler<TResult>(ILogger logger) =>
        GetRetryAndCircuitBreakerExceptionAsyncPolicyAndHandler<TResult, HttpRequestException>(logger, static exception => exception.ShouldHandleTransientHttpRequestException());

    public ResiliencePipelineWrapper<TResult> GetRetryAndCircuitBreakerExceptionAsyncPolicyAndHandler<TResult,
        TException>(ILogger logger, Func<TException, bool> predicate)
        where TException : Exception
    {
        var policy = GenericResiliencePipelines.GetResultHandleExceptionRetryAndCircuitBreaker<TResult, TException>(_configuration, predicate);

        return new ResiliencePipelineWrapper<TResult>(policy, logger);
    }

    public ResiliencePipelineWrapper GetRetryAndCircuitBreakerExceptionAsyncPolicyAndHandler<TException>(ILogger logger, Func<TException, bool> predicate)
        where TException : Exception
    {
        var policy = GenericResiliencePipelines.GetHandleExceptionRetryAndCircuitBreaker(_configuration, predicate);

        return new ResiliencePipelineWrapper(policy, logger);
    }

    public ResiliencePipelineWrapper<TResult> GetCircuitBreakerExceptionAsyncPolicyAndHandler<TResult,
        TException>(ILogger logger, Func<TException, bool> predicate)
        where TException : Exception
    {
        var policy = GenericResiliencePipelines.GetResultHandleExceptionCircuitBreaker<TResult, TException>(_configuration.CircuitBreakerPolicy, predicate);

        return new ResiliencePipelineWrapper<TResult>(policy, logger);
    }

    public ResiliencePipelineWrapper GetCircuitBreakerExceptionAsyncPolicyAndHandler<TException>(ILogger logger, Func<TException, bool> predicate)
        where TException : Exception
    {
        var policy = GenericResiliencePipelines.GetHandleExceptionCircuitBreaker(_configuration.CircuitBreakerPolicy, predicate);

        return new ResiliencePipelineWrapper(policy, logger);
    }

    public ResiliencePipelineWrapper<OneOf<TResult, NotFound, ApiError>> GetRetryAndCircuitBreakerOneOfResultWithNotFoundAsyncPolicyAndHandler<TResult>(ILogger logger)
    {
        var policy = ExtendedResiliencePipelines.GetRetryAndCircuitBreakerOneOfResultWithNotFoundAsyncPolicy<TResult>(_configuration);

        return new ResiliencePipelineWrapper<OneOf<TResult, NotFound, ApiError>>(policy, logger);
    }
}