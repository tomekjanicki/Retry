using Polly;
using Retry.Resiliency;

namespace Retry.Services;

public sealed class InternalApiClient : IInternalApiClient
{
    public const string Name = nameof(InternalApiClient);
    private const string Url = "/time?mode={0}";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Context _context;

    public InternalApiClient(IHttpClientFactory httpClientFactory, ILogger<InternalApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _context = HttpClientResiliencyHelper.GetContext(logger);
    }

    public async Task<string> GetTimeAsString(bool fail, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient(Name);
        var request = new HttpRequestMessage(HttpMethod.Get, string.Format(Url, fail ? "fail" : string.Empty));
        request.SetPolicyExecutionContext(_context);
        var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }
}