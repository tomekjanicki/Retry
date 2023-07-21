using Polly;
using Retry.Extensions;

namespace Retry.Services;

public sealed class InternalApiClient : IInternalApiClient
{
    public const string Name = nameof(InternalApiClient);
    private const string Url = "/time?mode={0}";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAsyncPolicy<HttpResponseMessage> _policy;

    public InternalApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _policy = PolicyHelper.GetRetryAndCircuitBreakerHttpRequestExceptionOrTransientHttpErrorAsyncPolicySimple();
    }

    public async Task<string> GetTimeAsString(bool fail, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient(Name);
        var response = await _policy.ExecuteAsync((fail, httpClient), static (p, token) => p.httpClient.GetAsync(string.Format(Url, p.fail ? "fail" : string.Empty), token), cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }
}