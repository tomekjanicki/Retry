using Polly;
using Retry.Resiliency.Models;

namespace Retry.Resiliency;

public static class HttpClientResiliencyHelper
{
    private static readonly IDictionary<string, ResiliencePipeline<HttpResponseMessage>> Pipelines = new Dictionary<string, ResiliencePipeline<HttpResponseMessage>>();

    public static ResiliencePipeline<HttpResponseMessage> GetSingleInstanceOfRetryAndCircuitBreakerAsyncPipeline(RetryAndCircuitBreakerPolicyConfiguration configuration, HttpRequestMessage message)
    {
        var key = message.GetKey();
        lock (Pipelines)
        {
            if (Pipelines.TryGetValue(key, out var value))
            {
                return value;
            }
            var pipeline = ExtendedResiliencePipelines.GetResultIsTransientHttpStatusCodeOrShouldHandleHttpRequestExceptionSocketErrorConnectionRefusedRetryAndCircuitBreaker(configuration);
            Pipelines.Add(key, pipeline);

            return pipeline;
        }
    }

    private static string GetKey(this HttpRequestMessage message)
    {
        var uri = message.RequestUri ?? throw new InvalidOperationException("Uri is null.");

        return $"{message.Method} {uri.Scheme}://{uri.Authority}{uri.AbsolutePath}";
    }
}