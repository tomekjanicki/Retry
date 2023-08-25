using System.Net;
using System.Net.Http;

namespace ApiClient.Extensions;

public static class HttpRequestExceptionExtensions
{
    private const string StatusCodeKeyName = "StatusCode";

    internal static void SetStatusCode(this HttpRequestException httpRequestException, HttpStatusCode httpStatusCode)
        => httpRequestException.Data[StatusCodeKeyName] = httpStatusCode;

    public static HttpStatusCode? GetStatusCode(this HttpRequestException httpRequestException) =>
        httpRequestException.Data.Contains(StatusCodeKeyName) && httpRequestException.Data[StatusCodeKeyName] is HttpStatusCode
            ? (HttpStatusCode)httpRequestException.Data[StatusCodeKeyName]
            : null;
}