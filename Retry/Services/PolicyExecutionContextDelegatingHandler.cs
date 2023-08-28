using Polly;
using Retry.Resiliency;

namespace Retry.Services;

public sealed class PolicyExecutionContextDelegatingHandler : DelegatingHandler
{
    private readonly Context _context;

    public PolicyExecutionContextDelegatingHandler(ILogger<PolicyExecutionContextDelegatingHandler> logger) => 
        _context = HttpClientResiliencyHelper.GetContext(logger);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.SetPolicyExecutionContext(_context);

        return base.SendAsync(request, cancellationToken);
    }
}