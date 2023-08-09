using OneOf;
using OneOf.Types;
using Polly;
using Retry.Extensions;
using Retry.Resiliency.Models;

namespace Retry.Resiliency;

public static class ResiliencyHelper
{
    public static Handlers<TResult> GetHandler<TResult>(ILogger logger) => new(logger, GetBreakLogResult<TResult>(), GetRetryLogResult<TResult>());

    public static Handlers GetHandler(ILogger logger) => new(logger, GetBreakLogResult(), GetRetryLogResult());

    public static Action<DelegateResult<TResult>, ILogger> GetRetryLogResult<TResult>() => GetLogResult<TResult>(true);

    public static Action<Exception, ILogger> GetRetryLogResult() => GetLogResult(true);

    public static Action<DelegateResult<TResult>, ILogger> GetBreakLogResult<TResult>() => GetLogResult<TResult>(false);

    public static Action<Exception, ILogger> GetBreakLogResult() => GetLogResult(false);


    public static IAsyncPolicy<OneOf<TResult, NotFound, ApiError>> GetRetryAndCircuitBreakerOneOfResultWithNotFoundAsyncPolicy<TResult>(RetryAndCircuitBreakerPolicyConfiguration configuration) =>
        GetRetryAndCircuitBreakerResultAsyncPolicy<OneOf<TResult, NotFound, ApiError>>(static of => of.ShouldHandleTransient(), configuration);

    public static IAsyncPolicy<TResult> GetRetryAndCircuitBreakerExceptionOrResultAsyncPolicy<TResult, TException>(Func<TException, bool> exceptionPredicate, Func<TResult, bool> resultPredicate, RetryAndCircuitBreakerPolicyConfiguration configuration)
        where TException : Exception
    {
        var builder = ResultHandleExceptionOrResult(exceptionPredicate, resultPredicate);

        return builder.GetRetryAndCircuitBreakerAsyncPolicy(configuration);
    }

    public static IAsyncPolicy<TResult> GetRetryAndCircuitBreakerResultAsyncPolicy<TResult>(Func<TResult, bool> resultPredicate, RetryAndCircuitBreakerPolicyConfiguration configuration)
    {
        var builder = ResultHandleResult(resultPredicate);

        return builder.GetRetryAndCircuitBreakerAsyncPolicy(configuration);
    }

    public static IAsyncPolicy<TResult> GetRetryAndCircuitBreakerExceptionAsyncPolicy<TResult, TException>(RetryAndCircuitBreakerPolicyConfiguration configuration, Func<TException, bool> predicate)
        where TException : Exception
    {
        var builder = ResultHandleException<TResult, TException>(predicate);

        return builder.GetRetryAndCircuitBreakerAsyncPolicy(configuration);
    }

    public static IAsyncPolicy<TResult> GetCircuitBreakerExceptionAsyncPolicy<TResult, TException>(CircuitBreakerPolicyConfiguration configuration, Func<TException, bool> predicate)
        where TException : Exception
    {
        var builder = ResultHandleException<TResult, TException>(predicate);

        return builder.GetCircuitBreakerAsyncPolicy(configuration);
    }

    public static IAsyncPolicy GetCircuitBreakerExceptionAsyncPolicy<TException>(CircuitBreakerPolicyConfiguration configuration, Func<TException, bool> predicate)
        where TException : Exception
    {
        var builder = HandleException(predicate);

        return builder.GetCircuitBreakerAsyncPolicy(configuration);
    }

    public static IAsyncPolicy GetRetryAndCircuitBreakerExceptionAsyncPolicy<TException>(RetryAndCircuitBreakerPolicyConfiguration configuration, Func<TException, bool> predicate)
        where TException : Exception
    {
        var builder = HandleException(predicate);

        return builder.GetRetryAndCircuitBreakerAsyncPolicy(configuration);
    }

    private static PolicyBuilder HandleException<TException>(Func<TException, bool> predicate)
        where TException : Exception =>
        Policy.Handle(predicate);

    private static PolicyBuilder<TResult> ResultHandleException<TResult, TException>(Func<TException, bool> predicate)
        where TException : Exception =>
        Policy<TResult>.Handle(predicate);

    private static PolicyBuilder<TResult> ResultHandleExceptionOrResult<TResult, TException>(Func<TException, bool> exceptionPredicate, Func<TResult, bool> resultPredicate)
        where TException : Exception =>
        Policy<TResult>.Handle(exceptionPredicate).OrResult(resultPredicate);

    private static PolicyBuilder<TResult> ResultHandleResult<TResult>(Func<TResult, bool> resultPredicate) =>
        Policy<TResult>.HandleResult(resultPredicate);

    private static bool ShouldHandleTransient<TResult>(this OneOf<TResult, NotFound, ApiError> of) =>
        of is { IsT0: false, IsT1: false } && of.AsT2.Transient;

    private static Action<DelegateResult<TResult>, ILogger> GetLogResult<TResult>(bool retry) =>
        retry ? static (result, logger) => logger.RetryLogResult(result) : static (result, logger) => logger.BreakLogResult(result);

    private static Action<Exception, ILogger> GetLogResult(bool retry) =>
        retry ? static (result, logger) => logger.RetryLogResult(result) : static (result, logger) => logger.BreakLogResult(result);
}