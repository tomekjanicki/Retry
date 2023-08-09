﻿using OneOf;
using OneOf.Types;
using Polly;
using Polly.CircuitBreaker;
using Retry.Extensions;
using Retry.Resiliency;

namespace Retry.Services;

public sealed class InternalApiClient : IInternalApiClient
{
    public const string Name = nameof(InternalApiClient);
    private const string GetTimeAsStringUrl = "/time?mode={0}";
    private const string GetUserFullNameByIdUrl = "/user?id={0}&mode={1}";

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
        var request = new HttpRequestMessage(HttpMethod.Get, string.Format(GetTimeAsStringUrl, fail ? "fail" : string.Empty));
        request.SetPolicyExecutionContext(_context);
        var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<OneOf<string, NotFound, ApiError>> GetUserFullNameById(int id, bool fail, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient(Name);
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, string.Format(GetUserFullNameByIdUrl, id, fail ? "fail" : string.Empty));
            request.SetPolicyExecutionContext(_context);
            var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            return await response.HandleWithNotFound<string, User>(static user => $"{user.FirstName} {user.LastName}", cancellationToken)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException e) when (e.ShouldHandleHttpRequestExceptionSocketErrorConnectionRefused())
        {
            return new ApiError(e.Message, true, null);
        }
        catch (BrokenCircuitException)
        {
            return Constants.CircuitOpenApiError;
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