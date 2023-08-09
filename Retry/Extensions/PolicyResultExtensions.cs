using OneOf;
using OneOf.Types;
using Polly;

namespace Retry.Extensions;

public static class PolicyResultExtensions
{
    public static OneOf<TResult, NotFound, ApiError> HandleFailureAndSuccess<TResult>(this PolicyResult<OneOf<TResult, NotFound, ApiError>> result, ApiError error, Func<Exception, bool> exceptionPredicate) =>
        result.Outcome switch
        {
            OutcomeType.Successful => result.Result,
            _ => result.FaultType is not null && result.FaultType.Value == FaultType.UnhandledException && exceptionPredicate(result.FinalException)
                ? error : result.FinalHandledResult
        };
}