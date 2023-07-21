using System.Net;

namespace Retry;

public sealed record Error(HttpStatusCode StatusCode, string Message);