using System.Net.Http.Json;
using System.Text.Json;
using OneOf;
using OneOf.Types;
using Retry.Extensions;

namespace Retry.Services;

public sealed class ExternalApiClient : IExternalApiClient
{
    public const string Name = nameof(ExternalApiClient);
    private const string GetTimeAsStringUrl = "/time?mode={0}";
    private const string GetItemsUrl = "/data?mode={0}";
    private const string GetUserFullNameByIdUrl = "/user?id={0}&mode={1}";

    private static readonly JsonSerializerOptions CamelCaseJsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IHttpClientFactory _httpClientFactory;

    public ExternalApiClient(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public async Task<string> GetTimeAsString(bool fail, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient(Name);
        var response = await httpClient.GetAsync(string.Format(GetTimeAsStringUrl, fail ? "fail" : string.Empty), cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<string>> GetItems(bool fail, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient(Name);
        var response = await httpClient.GetAsync(string.Format(GetItemsUrl, fail ? "fail" : string.Empty), cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<string>>(CamelCaseJsonSerializerOptions, cancellationToken).ConfigureAwait(false);

        return result ?? Array.Empty<string>();
    }

    public async Task<OneOf<string, NotFound, Error>> GetUserFullNameById(int id, bool fail, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient(Name);
        var response = await httpClient.GetAsync(string.Format(GetUserFullNameByIdUrl, id, fail ? "fail" : string.Empty), cancellationToken).ConfigureAwait(false);

        return await response.HandleWithNotFound<string, User>(user => $"{user.FirstName} {user.LastName}", cancellationToken).ConfigureAwait(false);
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