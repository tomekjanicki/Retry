using OneOf.Types;
using OneOf;
using Polly;
using Retry.Extensions;
using Retry.Resiliency.Models;

namespace Retry.Resiliency;

public static class ExtendedResiliencePipelines
{
    public static ResiliencePipeline<HttpResponseMessage> HandleResultIsTransientHttpStatusCodeOrShouldHandleHttpRequestExceptionSocketErrorConnectionRefusedRetryAndCircuitBreaker(
            RetryAndCircuitBreakerPolicyConfiguration configuration) =>
        GenericResiliencePipelines.HandleResultRetryAndCircuitBreaker<HttpResponseMessage>(
            configuration,
            static arguments => arguments.Outcome.ShouldHandleResultOrException<HttpResponseMessage, HttpRequestException>(
                static message => message.StatusCode.IsTransientHttpStatusCode(),
                static exception => exception.ShouldHandleHttpRequestExceptionSocketErrorConnectionRefused()),
            static arguments => arguments.Outcome.ShouldHandleResultOrException<HttpResponseMessage, HttpRequestException>(
                static message => message.StatusCode.IsTransientHttpStatusCode(),
                static exception => exception.ShouldHandleHttpRequestExceptionSocketErrorConnectionRefused()));

    public static ResiliencePipeline<OneOf<TResult, NotFound, ApiError>> HandleOneOfResultWithNotFoundRetryAndCircuitBreaker<TResult>(RetryAndCircuitBreakerPolicyConfiguration configuration) =>
        GenericResiliencePipelines.HandleResultRetryAndCircuitBreaker<OneOf<TResult, NotFound, ApiError>>(configuration, static arguments => arguments.Outcome.ShouldHandleResult(static of => of.ShouldHandleTransient()),
            static arguments => arguments.Outcome.ShouldHandleResult(static of => of.ShouldHandleTransient()));

    private static bool ShouldHandleTransient<TResult>(this OneOf<TResult, NotFound, ApiError> of) =>
        of is { IsT0: false, IsT1: false } && of.AsT2.Transient;
}