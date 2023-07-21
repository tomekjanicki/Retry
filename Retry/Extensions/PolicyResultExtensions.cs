using System.Net;
using OneOf;
using OneOf.Types;
using Polly;

namespace Retry.Extensions;

public static class PolicyResultExtensions
{
    public static OneOf<TResult, NotFound, Error> HandleFailureAndSuccess<TResult>(this PolicyResult<OneOf<TResult, NotFound, Error>> result) => 
        result.Outcome == OutcomeType.Failure ? new Error(HttpStatusCode.ServiceUnavailable, "Circuit breaker open") : result.Result;
}