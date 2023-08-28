using ApiClient.Extensions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ApiClient.Services;

public sealed class InternalApiClientNetStandard : IInternalApiClientNetStandard
{
    public const string Name = nameof(InternalApiClientNetStandard);
    private const string GetTimeAsStringUrl = "/time?mode={0}";

    private readonly IHttpClientFactory _httpClientFactory;

    public InternalApiClientNetStandard(IHttpClientFactory httpClientFactory) =>
        _httpClientFactory = httpClientFactory;

    public async Task<string> GetTimeAsString(bool fail, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient(Name);
        var request = new HttpRequestMessage(HttpMethod.Get, string.Format(GetTimeAsStringUrl, fail ? "fail" : string.Empty));
        var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await response.EnsureSuccessStatusCodeWithContentInfo().ConfigureAwait(false);

        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }
}