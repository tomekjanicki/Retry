using OneOf.Types;
using OneOf;
using Polly;
using Retry.Extensions;
using Retry.Resiliency.Models;

namespace Retry.Resiliency;

public static class ExtendedResiliencePipelines
{
    public static ResiliencePipeline<HttpResponseMessage> GetResultIsTransientHttpStatusCodeOrShouldHandleHttpRequestExceptionSocketErrorConnectionRefusedRetryAndCircuitBreaker(RetryAndCircuitBreakerPolicyConfiguration configuration) =>
        GenericResiliencePipelines.GetResultHandleExceptionOrResultRetryAndCircuitBreaker<HttpResponseMessage, HttpRequestException>(configuration, 
            static message => message.StatusCode.IsTransientHttpStatusCode(),
            static exception => exception.ShouldHandleHttpRequestExceptionSocketErrorConnectionRefused());

    public static ResiliencePipeline<OneOf<TResult, NotFound, ApiError>> GetOneOfResultWithNotFoundRetryAndCircuitBreaker<TResult>(RetryAndCircuitBreakerPolicyConfiguration configuration) =>
        GenericResiliencePipelines.GetResultHandleResultRetryAndCircuitBreaker<OneOf<TResult, NotFound, ApiError>>(configuration, static of => of.ShouldHandleTransient());

    private static bool ShouldHandleTransient<TResult>(this OneOf<TResult, NotFound, ApiError> of) =>
        of is { IsT0: false, IsT1: false } && of.AsT2.Transient;
}