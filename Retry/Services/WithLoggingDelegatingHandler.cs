using Retry.Resiliency;
using Retry.Resiliency.Models;

namespace Retry.Services;

public sealed class WithLoggingDelegatingHandler : DelegatingHandler
{
    private readonly ILogger<WithLoggingDelegatingHandler> _logger;
    private readonly RetryAndCircuitBreakerPolicyConfiguration _configuration;

    public WithLoggingDelegatingHandler(RetryAndCircuitBreakerPolicyConfiguration configuration, ILogger<WithLoggingDelegatingHandler> logger)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var pipeline = HttpClientResiliencyHelper.GetRetryAndCircuitBreakerPipeline(_configuration, request);
        var wrapper = new ResiliencePipelineWrapper<HttpResponseMessage>(pipeline, _logger);

        return wrapper.ExecuteAsync((request, cancellationToken, This: this), static (p, token) => p.This.SendAsyncCore(p.request, token), cancellationToken);
    }

    private Task<HttpResponseMessage> SendAsyncCore(HttpRequestMessage request, CancellationToken cancellationToken)
        => base.SendAsync(request, cancellationToken);
}