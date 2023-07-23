using System.Net;
using OneOf;
using OneOf.Types;
using Retry.Infrastructure;

namespace Retry.Extensions;

public static class PolicyAndHandlerWrapperExtensions
{
    public static Error CircuitBreakerOpenError = new(HttpStatusCode.ServiceUnavailable, "Circuit breaker open.");

    public static async Task<OneOf<TResult, NotFound, Error>> ExecuteAsResult<TResult, TParam>(this AsyncPolicyAndHandlerWrapper<OneOf<TResult, NotFound, Error>> wrapper, TParam param, Error error,
        Func<TParam, CancellationToken, Task<OneOf<TResult, NotFound, Error>>> func, CancellationToken cancellationToken)
        where TParam : struct
    {
        var result = await wrapper.ExecuteAndCaptureAsync(param, func, cancellationToken).ConfigureAwait(false);

        return result.HandleFailureAndSuccess(error);
    }
}