using Polly;
using Retry.Resiliency.Models;

namespace Retry.Resiliency;

public static class HttpClientResiliencyHelper
{
    private static readonly IDictionary<string, ResiliencePipeline<HttpResponseMessage>> Pipelines = new Dictionary<string, ResiliencePipeline<HttpResponseMessage>>();

    public static ResiliencePipeline<HttpResponseMessage> GetRetryAndCircuitBreakerPipeline(RetryAndCircuitBreakerPolicyConfiguration configuration, HttpRequestMessage message) =>
        PoolHelper.GetOrCreate(Pipelines, message.GetKey(), configuration, 
            static p => ExtendedResiliencePipelines.HandleResultIsTransientHttpStatusCodeOrShouldHandleHttpRequestExceptionSocketErrorConnectionRefusedRetryAndCircuitBreaker(p));

    private static string GetKey(this HttpRequestMessage message)
    {
        var uri = message.RequestUri ?? throw new InvalidOperationException("Uri is null.");

        return $"{message.Method} {uri.Scheme}://{uri.Authority}{uri.AbsolutePath}";
    }
}