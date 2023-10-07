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
}