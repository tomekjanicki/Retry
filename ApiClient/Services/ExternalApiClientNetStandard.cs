using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApiClient.Extensions;

namespace ApiClient.Services;

public sealed class ExternalApiClientNetStandard : IExternalApiClientNetStandard
{
    public const string Name = nameof(ExternalApiClientNetStandard);
    private const string GetTimeAsStringUrl = "/time?mode={0}";

    private readonly IHttpClientFactory _httpClientFactory;

    public ExternalApiClientNetStandard(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public async Task<string> GetTimeAsString(bool fail, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient(Name);
        var response = await httpClient.GetAsync(string.Format(GetTimeAsStringUrl, fail ? "fail" : string.Empty), cancellationToken).ConfigureAwait(false);
        await response.EnsureSuccessStatusCodeWithContentInfo().ConfigureAwait(false);

        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }
}