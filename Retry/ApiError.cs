using System.Net;

namespace Retry;

public sealed record ApiError(string Message, bool Transient, HttpStatusCode? StatusCode);