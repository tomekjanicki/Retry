using OneOf;
using OneOf.Types;
using Polly;

namespace Retry.Extensions;

public static class OutcomeExtensions
{
    public static OneOf<TResult, NotFound, ApiError> HandleFailureAndSuccess<TResult>(this Outcome<OneOf<TResult, NotFound, ApiError>> result, ApiError error, Func<Exception, bool> exceptionPredicate)
    {
        if (result.Exception is not null)
        {
            return exceptionPredicate(result.Exception) ? error : result.Result;
        }

        return result.Result;
    }

    public static ValueTask<bool> ShouldHandleResultOrException<TResult, TException>(this Outcome<TResult> outcome, Func<TResult, bool> resultPredicate, Func<TException, bool> exceptionPredicate)
    {
        if (outcome.Exception is TException exception)
        {
            return new ValueTask<bool>(exceptionPredicate(exception));
        }
        var result = outcome.Result;

        return result is not null ? new ValueTask<bool>(resultPredicate(result)) : new ValueTask<bool>(false);
    }

    public static ValueTask<bool> ShouldHandleException<TResult, TException>(this Outcome<TResult> outcome, Func<TException, bool> exceptionPredicate)
    {
        if (outcome.Exception is TException exception)
        {
            return new ValueTask<bool>(exceptionPredicate(exception));
        }

        return new ValueTask<bool>(false);
    }

    public static ValueTask<bool> ShouldHandleResult<TResult>(this Outcome<TResult> outcome, Func<TResult, bool> resultPredicate)
    {
        var result = outcome.Result;

        return result is not null ? new ValueTask<bool>(resultPredicate(result)) : new ValueTask<bool>(false);
    }
}