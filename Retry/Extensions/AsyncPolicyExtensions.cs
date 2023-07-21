using Polly;

namespace Retry.Extensions;

public static class AsyncPolicyExtensions
{
    private const string Param = "param";
    private const string Func = "func";

    public static Task<TResult> ExecuteAsync<TResult, TParam>(this IAsyncPolicy<TResult> asyncPolicy, TParam param,
        Func<TParam, CancellationToken, Task<TResult>> func, CancellationToken cancellationToken)
        where TParam : struct
    {
        var context = GetContext(param, func);

        return asyncPolicy.ExecuteAsync(static (context, token) => context.GetValue<Func<TParam, CancellationToken, Task<TResult>>>(Func)(context.GetValue<TParam>(Param), token), context, cancellationToken);
    }

    public static Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult, TParam>(this IAsyncPolicy<TResult> asyncPolicy, TParam param,
        Func<TParam, CancellationToken, Task<TResult>> func, CancellationToken cancellationToken)
        where TParam : struct
    {
        var context = GetContext(param, func);

        return asyncPolicy.ExecuteAndCaptureAsync(static (context, token) => context.GetValue<Func<TParam, CancellationToken, Task<TResult>>>(Func)(context.GetValue<TParam>(Param), token), context, cancellationToken);
    }

    private static Context GetContext<TResult, TParam>(TParam param, Func<TParam, CancellationToken, Task<TResult>> func) where TParam : struct =>
        new(string.Empty, new Dictionary<string, object>
        {
            { Param, param },
            { Func, func }
        });
}