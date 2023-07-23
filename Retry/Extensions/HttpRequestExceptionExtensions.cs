using System.Net.Sockets;

namespace Retry.Extensions;

public static class HttpRequestExceptionExtensions
{
    public static bool ShouldHandleTransientHttpRequestException(this HttpRequestException httpRequestException) =>
        httpRequestException.StatusCode is not null && httpRequestException.StatusCode.Value.IsTransientHttpStatusCode()
        || httpRequestException.ShouldHandleHttpRequestExceptionSocketErrorConnectionRefused();

    public static bool ShouldHandleHttpRequestExceptionSocketErrorConnectionRefused(this HttpRequestException httpRequestException) =>
        httpRequestException.StatusCode is null && httpRequestException.InnerException is SocketException { SocketErrorCode: SocketError.ConnectionRefused };
}