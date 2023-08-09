using System.Net;
using System.Net.Http.Json;
using OneOf;
using OneOf.Types;
using Retry.Resiliency;

namespace Retry.Extensions;

public static class HttpResponseMessageExtensions
{

    public static async Task<OneOf<TResult, NotFound, ApiError>> HandleWithNotFound<TResult, TIntermediateResult>(
        this HttpResponseMessage message, Func<TIntermediateResult, TResult> converter, CancellationToken cancellationToken)
    {
        switch (message.StatusCode)
        {
            case HttpStatusCode.OK:
            {
                var result = await message.Content.ReadFromJsonAsync<TIntermediateResult>(Constants.CamelCaseJsonSerializerOptions, cancellationToken).ConfigureAwait(false);

                return result is null ? Constants.ResultNullError : converter(result);
            }
            case HttpStatusCode.NotFound:
            {
                return new NotFound();
            }
            default:
            {
                var content = await message.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                return new ApiError(content, message.StatusCode.IsTransientHttpStatusCode(), message.StatusCode);
            }
        }
    }
}