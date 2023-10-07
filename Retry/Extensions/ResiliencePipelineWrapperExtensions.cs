using OneOf;
using OneOf.Types;
using Polly.CircuitBreaker;
using Retry.Resiliency;

namespace Retry.Extensions;

public static class ResiliencePipelineWrapperExtensions
{
    public static readonly Func<Exception, bool> CircuitOpenPredicate = exception => exception is BrokenCircuitException;

    public static async Task<OneOf<TResult, NotFound, ApiError>> ExecuteAsResult<TResult, TParam>(this ResiliencePipelineWrapper<OneOf<TResult, NotFound, ApiError>> wrapper, TParam param, ApiError error, Func<Exception, bool> exceptionPredicate,
        Func<TParam, CancellationToken, Task<OneOf<TResult, NotFound, ApiError>>> func, CancellationToken cancellationToken)
        where TParam : struct
    {
        var result = await wrapper.ExecuteOutcomeAsync(param, func, cancellationToken).ConfigureAwait(false);

        return result.HandleFailureAndSuccess(error, exceptionPredicate);
    }
}