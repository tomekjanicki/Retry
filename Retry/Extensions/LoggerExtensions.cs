using Polly;

namespace Retry.Extensions;

public static class LoggerExtensions
{
    public static void RetryLogResult<TResult>(this ILogger logger, Outcome<TResult> outcome) =>
        LogResult(logger, outcome, static (logger, exception) => logger.LogError(exception, "Exception during retry."),
            static (logger, result) => logger.LogError("Error during retry. {Result}", result));

    public static void OpenLogResult<TResult>(this ILogger logger, Outcome<TResult> outcome) =>
        LogResult(logger, outcome, static (logger, exception) => logger.LogError(exception, "Exception during open."),
            static (logger, result) => logger.LogError("Error during open. {Result}", result));

    private static void LogResult<TResult>(this ILogger logger, Outcome<TResult> outcome, Action<ILogger, Exception> exceptionAction, Action<ILogger, TResult> resultAction)
    {
        var exception = outcome.Exception;
        if (exception is not null)
        {
            exceptionAction(logger, exception);

            return;
        }
        var result = outcome.Result;
        if (result is null)
        {
            return;
        }
        resultAction(logger, result);
    }
}