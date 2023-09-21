using Polly;

namespace Retry.Extensions;

public static class LoggerExtensions
{
    public static void RetryLogResult<TResult>(this ILogger logger, DelegateResult<TResult> result)
    {
        if (result.Exception is not null)
        {
            logger.LogError(result.Exception, "Exception during retry.");

            return;
        }

        logger.LogError("Error during retry. {Result}", result.Result);
    }

    public static void RetryLogResult(this ILogger logger, Exception result) =>
        logger.LogError(result, "Exception during retry.");

    public static void BreakLogResult<TResult>(this ILogger logger, DelegateResult<TResult> result)
    {
        if (result.Exception is not null)
        {
            logger.LogError(result.Exception, "Exception during break.");

            return;
        }

        logger.LogError("Error during break. {Result}", result.Result);
    }

    public static void BreakLogResult(this ILogger logger, Exception result) => 
        logger.LogError(result, "Exception during break.");
}