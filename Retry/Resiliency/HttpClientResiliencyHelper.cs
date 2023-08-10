using Polly;
using Retry.Extensions;
using Retry.Resiliency.Models;

namespace Retry.Resiliency;

public static class HttpClientResiliencyHelper
{
    public static Context GetContext(ILogger logger) =>
        GetContext(new Handlers<HttpResponseMessage>(logger, ResiliencyHelper.GetBreakLogResult<HttpResponseMessage>(),
            ResiliencyHelper.GetRetryLogResult<HttpResponseMessage>()));

    private static readonly IDictionary<string, IAsyncPolicy<HttpResponseMessage>> Policies = new Dictionary<string, IAsyncPolicy<HttpResponseMessage>>();

    public static IAsyncPolicy<HttpResponseMessage> GetSingleInstanceOfRetryAndCircuitBreakerAsyncPolicy(RetryAndCircuitBreakerPolicyConfiguration configuration, HttpRequestMessage message)
    {
        var key = GetKey(message);
        lock (Policies)
        {
            if (Policies.TryGetValue(key, out var value))
            {
                return value;
            }
            var policy = GetRetryAndCircuitBreakerAsyncPolicy(configuration);
            Policies.Add(key, policy);

            return policy;
        }
    }

    private static string GetKey(HttpRequestMessage message)
    {
        var uri = message.RequestUri ?? throw new InvalidOperationException("Uri is null.");

        return $"{uri.Scheme}://{uri.Authority}{uri.AbsolutePath}";
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryAndCircuitBreakerAsyncPolicy(RetryAndCircuitBreakerPolicyConfiguration configuration) =>
        ResiliencyHelper.GetRetryAndCircuitBreakerExceptionOrResultAsyncPolicy<HttpResponseMessage, HttpRequestException>(
            static exception => exception.ShouldHandleHttpRequestExceptionSocketErrorConnectionRefused(),
            static message => message.StatusCode.IsTransientHttpStatusCode(), configuration);

    private static Context GetContext<TResult>(Handlers<TResult> handlers) =>
        new(string.Empty, new Dictionary<string, object>
        {
            { PolicyBuilderExtensions.Handlers, handlers }
        });
}