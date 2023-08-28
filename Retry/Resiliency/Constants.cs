using System.Text.Json;

namespace Retry.Resiliency;

public static class Constants
{
    public static readonly JsonSerializerOptions CamelCaseJsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static readonly ApiError CircuitOpenApiError = new("Circuit is open.", true, null);

    public static readonly ApiError ResultNullApiError = new("Returned value was null.", false, null);

    public static readonly ApiError ServiceNotAvailableApiError = new("Service is not available.", true, null);
}