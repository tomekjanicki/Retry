using Microsoft.Extensions.Options;
using Retry.Resiliency.Models;
using Retry.Services;

namespace Retry.Extensions;

public static class HttpClientBuilderExtensions
{
    public static IHttpClientBuilder AddWithLoggingDelegatingHandler(this IHttpClientBuilder builder) =>
        builder.AddHttpMessageHandler(static provider =>
        {
            var factory = provider.GetRequiredService<ILoggerFactory>();
            var logger = factory.CreateLogger<WithLoggingDelegatingHandler>();
            var configuration = provider.GetRequiredService<IOptions<RetryAndCircuitBreakerPolicyConfiguration>>().Value;

            return new WithLoggingDelegatingHandler(configuration, logger);
        });
}