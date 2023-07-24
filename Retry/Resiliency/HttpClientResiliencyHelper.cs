using Polly;
using Retry.Extensions;
using Retry.Resiliency.Models;

namespace Retry.Resiliency;

public static class HttpClientResiliencyHelper
{
    public static Context GetContext(ILogger logger) =>
        GetContext(new Handlers<HttpResponseMessage>(logger, ResiliencyHelper.GetBreakLogResult<HttpResponseMessage>(),
            ResiliencyHelper.GetRetryLogResult<HttpResponseMessage>()));

    public static IAsyncPolicy<HttpResponseMessage> GetRetryAndCircuitBreakerAsyncPolicy(RetryAndCircuitBreakerPolicyConfiguration configuration) =>
        ResiliencyHelper.GetRetryAndCircuitBreakerExceptionOrResultAsyncPolicy<HttpResponseMessage, HttpRequestException>(
            static exception => exception.ShouldHandleHttpRequestExceptionSocketErrorConnectionRefused(),
            static message => message.StatusCode.IsTransientHttpStatusCode(), configuration);

    private static Context GetContext<TResult>(Handlers<TResult> handlers) =>
        new(string.Empty, new Dictionary<string, object>
        {
            { PolicyBuilderExtensions.Handlers, handlers }
        });
}