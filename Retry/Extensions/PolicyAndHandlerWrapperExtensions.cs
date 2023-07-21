using OneOf;
using OneOf.Types;

namespace Retry.Extensions;

public static class PolicyAndHandlerWrapperExtensions
{
    public static async Task<OneOf<TResult, NotFound, Error>> ExecuteAsResult<TResult, TParam>(this PolicyAndHandlerWrapper<OneOf<TResult, NotFound, Error>> wrapper, TParam param,
        Func<TParam, CancellationToken, Task<OneOf<TResult, NotFound, Error>>> func, CancellationToken cancellationToken)
        where TParam : struct
    {
        var result = await wrapper.ExecuteAndCaptureAsync(param, func, cancellationToken).ConfigureAwait(false);

        return result.HandleFailureAndSuccess();
    }
}