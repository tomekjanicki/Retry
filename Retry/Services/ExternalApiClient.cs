using System.Net.Http.Json;
using OneOf;
using OneOf.Types;
using Retry.Extensions;
using Retry.Resiliency;

namespace Retry.Services;

public sealed class ExternalApiClient : IExternalApiClient
{
    public const string Name = nameof(ExternalApiClient);
    private const string GetTimeAsStringUrl = "/time?mode={0}";
    private const string GetItemsUrl = "/data?mode={0}";
    private const string GetUserFullNameByIdUrl = "/user?id={0}&mode={1}";

    private readonly IHttpClientFactory _httpClientFactory;

    public ExternalApiClient(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public async Task<string> GetTimeAsString(bool fail, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient(Name);
        var response = await httpClient.GetAsync(string.Format(GetTimeAsStringUrl, fail ? "fail" : string.Empty), cancellationToken).ConfigureAwait(false);
        await response.EnsureSuccessStatusCodeWithContentInfo(cancellationToken).ConfigureAwait(false);

        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<string>> GetItems(bool fail, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient(Name);
        var response = await httpClient.GetAsync(string.Format(GetItemsUrl, fail ? "fail" : string.Empty), cancellationToken).ConfigureAwait(false);
        await response.EnsureSuccessStatusCodeWithContentInfo(cancellationToken).ConfigureAwait(false);
        var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<string>>(Constants.CamelCaseJsonSerializerOptions, cancellationToken).ConfigureAwait(false);

        return result ?? Array.Empty<string>();
    }

    public async Task<OneOf<string, NotFound, ApiError>> GetUserFullNameById(int id, bool fail, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient(Name);
        try
        {
            var response = await httpClient.GetAsync(string.Format(GetUserFullNameByIdUrl, id, fail ? "fail" : string.Empty), cancellationToken).ConfigureAwait(false);

            return await response.HandleWithNotFound<string, User>(static user => $"{user.FirstName} {user.LastName}", cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException e) when (e.ShouldHandleHttpRequestExceptionSocketErrorConnectionRefused())
        {
            return Constants.ServiceNotAvailableApiError;
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class User
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public string FirstName { get; init; } = string.Empty;

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public string LastName { get; init; } = string.Empty;
    }
}