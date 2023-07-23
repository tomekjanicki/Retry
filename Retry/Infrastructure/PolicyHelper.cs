using OneOf.Types;
using OneOf;
using Polly;
using Polly.Extensions.Http;
using Retry.Extensions;
using Retry.Infrastructure.Models;

namespace Retry.Infrastructure;

public static class PolicyHelper
{
    public static Handlers<TResult> GetHandler<TResult>(ILogger logger) => new(logger, GetBreakLogResult<TResult>(), GetRetryLogResult<TResult>());

    public static Handlers GetHandler(ILogger logger) => new(logger, GetBreakLogResult(), GetRetryLogResult());

    public static Action<DelegateResult<TResult>, ILogger> GetRetryLogResult<TResult>() => GetLogResult<TResult>(true);

    public static Action<Exception, ILogger> GetRetryLogResult() => GetLogResult(true);

    public static Action<DelegateResult<TResult>, ILogger> GetBreakLogResult<TResult>() => GetLogResult<TResult>(false);

    public static Action<Exception, ILogger> GetBreakLogResult() => GetLogResult(false);

    public static IAsyncPolicy<TResult> GetRetryAndCircuitBreakerExceptionAsyncPolicySimple<TResult, TException>(Func<TException, bool> predicate)
        where TException : Exception
    {
        var builder = ResultHandleException<TResult, TException>(predicate);

        return builder.GetRetryAndCircuitBreakerAsyncPolicy();
    }

    public static IAsyncPolicy GetCircuitBreakerExceptionAsyncPolicySimple<TException>(Func<TException, bool> predicate)
        where TException : Exception
    {
        var builder = HandleException(predicate);

        return builder.GetCircuitBreakerAsyncPolicy();
    }

    public static IAsyncPolicy GetRetryAndCircuitBreakerExceptionAsyncPolicySimple<TException>(Func<TException, bool> predicate)
        where TException : Exception
    {
        var builder = HandleException(predicate);

        return builder.GetRetryAndCircuitBreakerAsyncPolicy();
    }

    public static IAsyncPolicy<TResult> GetCircuitBreakerExceptionAsyncPolicySimple<TResult, TException>(Func<TException, bool> predicate)
        where TException : Exception
    {
        var builder = ResultHandleException<TResult, TException>(predicate);

        return builder.GetCircuitBreakerAsyncPolicy();
    }

    public static IAsyncPolicy<OneOf<TResult, NotFound, Error>> GetRetryAndCircuitBreakerTransientHttpRequestExceptionOrOneOfResultWithNotFoundAsyncPolicySimple<TResult>() =>
        GetRetryAndCircuitBreakerExceptionOrResultAsyncPolicySimple<OneOf<TResult, NotFound, Error>, HttpRequestException>(static exception => exception.ShouldHandleHttpRequestExceptionSocketErrorConnectionRefused(), static of => of.ShouldHandleTransientHttpStatusCode());

    public static IAsyncPolicy<TResult> GetRetryAndCircuitBreakerExceptionOrResultAsyncPolicySimple<TResult, TException>(Func<TException, bool> exceptionPredicate, Func<TResult, bool> resultPredicate)
        where TException : Exception
    {
        var builder = ResultHandleExceptionOrResult(exceptionPredicate, resultPredicate);

        return builder.GetRetryAndCircuitBreakerAsyncPolicy();
    }

    public static IAsyncPolicy<OneOf<TResult, NotFound, Error>> GetRetryAndCircuitBreakerTransientHttpRequestExceptionOrOneOfResultWithNotFoundAsyncPolicy<TResult>(RetryAndCircuitBreakerPolicyConfiguration configuration) =>
        GetRetryAndCircuitBreakerExceptionOrResultAsyncPolicy<OneOf<TResult, NotFound, Error>, HttpRequestException>(static exception => exception.ShouldHandleHttpRequestExceptionSocketErrorConnectionRefused(), static of => of.ShouldHandleTransientHttpStatusCode(), configuration);

    public static IAsyncPolicy<TResult> GetRetryAndCircuitBreakerExceptionOrResultAsyncPolicy<TResult, TException>(Func<TException, bool> exceptionPredicate, Func<TResult, bool> resultPredicate, RetryAndCircuitBreakerPolicyConfiguration configuration)
        where TException : Exception
    {
        var builder = ResultHandleExceptionOrResult(exceptionPredicate, resultPredicate);

        return builder.GetRetryAndCircuitBreakerAsyncPolicy(configuration);
    }

    public static IAsyncPolicy<HttpResponseMessage> GetRetryAndCircuitBreakerHttpRequestExceptionOrTransientHttpErrorAsyncPolicySimple()
    {
        var builder = HttpResponseMessageHandleHttpRequestExceptionOrTransientHttpError();

        return builder.GetRetryAndCircuitBreakerAsyncPolicy();
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

    private static PolicyBuilder<HttpResponseMessage> HttpResponseMessageHandleHttpRequestExceptionOrTransientHttpError() =>
        Policy<HttpResponseMessage>.Handle<HttpRequestException>(static exception => exception.ShouldHandleHttpRequestExceptionSocketErrorConnectionRefused()).OrTransientHttpError();

    private static bool ShouldHandleTransientHttpStatusCode<TResult>(this OneOf<TResult, NotFound, Error> of) =>
        of is { IsT0: false, IsT1: false } && of.AsT2.StatusCode.IsTransientHttpStatusCode();

    private static Action<DelegateResult<TResult>, ILogger> GetLogResult<TResult>(bool retry) =>
        retry ? static (result, logger) => logger.RetryLogResult(result) : static (result, logger) => logger.BreakLogResult(result);

    private static Action<Exception, ILogger> GetLogResult(bool retry) =>
        retry ? static (result, logger) => logger.RetryLogResult(result) : static (result, logger) => logger.BreakLogResult(result);
}