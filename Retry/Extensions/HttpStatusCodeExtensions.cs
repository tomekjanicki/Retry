using System.Net;

namespace Retry.Extensions;

public static class HttpStatusCodeExtensions
{
    public static bool IsTransientHttpStatusCode(this HttpStatusCode code) =>
        code is >= HttpStatusCode.InternalServerError or HttpStatusCode.RequestTimeout;
}