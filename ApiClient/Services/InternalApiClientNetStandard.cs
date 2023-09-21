using System.Globalization;
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

    public async Task<string> GetTimeAsString(bool fail, int? delayInMilliseconds, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(Name);
        var url = string.Format(CultureInfo.InvariantCulture, GetTimeAsStringUrl, fail ? "fail" : string.Empty);
        if (delayInMilliseconds is not null)
        {
            url = $"{url}&delay={delayInMilliseconds.Value}";
        }
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await response.EnsureSuccessStatusCodeWithContentInfo().ConfigureAwait(false);

        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }
}