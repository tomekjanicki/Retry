using Polly;

namespace Retry.Extensions;

public static class LoggerExtensions
{
    public static void RetryLogResult<TResult>(this ILogger logger, Outcome<TResult> outcome)
    {
        var exception = outcome.Exception;
        if (exception is not null)
        {
            logger.LogError(exception, "Exception during retry.");

            return;
        }
        var result = outcome.Result;
        if (result is null)
        {
            return;
        }
        logger.LogError("Error during retry. {Result}", result);
    }

    public static void OpenLogResult<TResult>(this ILogger logger, Outcome<TResult> outcome)
    {
        var exception = outcome.Exception;
        if (exception is not null)
        {
            logger.LogError(exception, "Exception during open.");

            return;
        }
        var result = outcome.Result;
        if (result is null)
        {
            return;
        }
        logger.LogError("Error during open. {Result}", result);
    }
}