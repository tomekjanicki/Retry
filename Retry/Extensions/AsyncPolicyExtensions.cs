using Polly;
using Retry.Resiliency.Models;

namespace Retry.Extensions;

public static class AsyncPolicyExtensions
{
    private const string Param = "param";
    private const string Func = "func";

    public static Task<TResult> ExecuteAsync<TResult, TParam>(this IAsyncPolicy<TResult> asyncPolicy, TParam param,
        Func<TParam, CancellationToken, Task<TResult>> func, Handlers<TResult>? handlers = null, CancellationToken cancellationToken = default)
        where TParam : struct
    {
        var context = GetContext(param, func, handlers);

        return asyncPolicy.ExecuteAsync(static (context, token) => context.GetValue<Func<TParam, CancellationToken, Task<TResult>>>(Func)(context.GetValue<TParam>(Param), token), context, cancellationToken);
    }

    public static Task ExecuteAsync<TParam>(this IAsyncPolicy asyncPolicy, TParam param,
        Func<TParam, CancellationToken, Task> func, Handlers? handlers = null, CancellationToken cancellationToken = default)
        where TParam : struct
    {
        var context = GetContext(param, func, handlers);

        return asyncPolicy.ExecuteAsync(static (context, token) => context.GetValue<Func<TParam, CancellationToken, Task>>(Func)(context.GetValue<TParam>(Param), token), context, cancellationToken);
    }

    public static Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult, TParam>(this IAsyncPolicy<TResult> asyncPolicy, TParam param,
        Func<TParam, CancellationToken, Task<TResult>> func, Handlers<TResult>? handlers = null, CancellationToken cancellationToken = default)
        where TParam : struct
    {
        var context = GetContext(param, func, handlers);

        return asyncPolicy.ExecuteAndCaptureAsync(static (context, token) => context.GetValue<Func<TParam, CancellationToken, Task<TResult>>>(Func)(context.GetValue<TParam>(Param), token), context, cancellationToken);
    }

    private static Context GetContext<TResult, TParam>(TParam param, Func<TParam, CancellationToken, Task<TResult>> func,
        Handlers<TResult>? handlers) where TParam : struct
    {
        var contextData = new Dictionary<string, object>
        {
            { Param, param },
            { Func, func }
        };
        if (handlers is not null)
        {
            contextData.Add(PolicyBuilderExtensions.Handlers, handlers);
        }

        return new Context(string.Empty, contextData);
    }

    private static Context GetContext<TParam>(TParam param, Func<TParam, CancellationToken, Task> func,
        Handlers? handlers) where TParam : struct
    {
        var contextData = new Dictionary<string, object>
        {
            { Param, param },
            { Func, func }
        };
        if (handlers is not null)
        {
            contextData.Add(PolicyBuilderExtensions.Handlers, handlers);
        }

        return new Context(string.Empty, contextData);
    }
}