using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using OneOf;
using OneOf.Types;

namespace Retry.Extensions;

public static class HttpResponseMessageExtensions
{
    private static readonly JsonSerializerOptions CamelCaseJsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly Error ResultNullError = new(HttpStatusCode.ExpectationFailed, "Returned value was null.");

    public static async Task<OneOf<TResult, NotFound, Error>> HandleWithNotFound<TResult, TIntermediateResult>(
        this HttpResponseMessage message, Func<TIntermediateResult, TResult> converter, CancellationToken cancellationToken)
    {
        switch (message.StatusCode)
        {
            case HttpStatusCode.OK:
            {
                var result = await message.Content.ReadFromJsonAsync<TIntermediateResult>(CamelCaseJsonSerializerOptions, cancellationToken).ConfigureAwait(false);

                return result is null ? ResultNullError : converter(result);
            }
            case HttpStatusCode.NotFound:
            {
                return new NotFound();
            }
            default:
            {
                var content = await message.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                return new Error(message.StatusCode, content);
            }
        }
    }
}