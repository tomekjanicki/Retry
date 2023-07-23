using OneOf;
using OneOf.Types;
using Polly;

namespace Retry.Extensions;

public static class PolicyResultExtensions
{
    public static OneOf<TResult, NotFound, Error> HandleFailureAndSuccess<TResult>(this PolicyResult<OneOf<TResult, NotFound, Error>> result, Error error) => 
        result.Outcome == OutcomeType.Failure ? error : result.Result;
}