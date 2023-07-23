using Polly;
using Retry.Extensions;
using Retry.Infrastructure.Models;

namespace Retry.Infrastructure;

public sealed class AsyncPolicyAndHandlerWrapper<TResult>
{
    private readonly IAsyncPolicy<TResult> _policy;
    private readonly Handlers<TResult> _handler;

    public AsyncPolicyAndHandlerWrapper(IAsyncPolicy<TResult> policy, Handlers<TResult> handler)
    {
        _policy = policy;
        _handler = handler;
    }

    public Task<TResult> ExecuteAsync<TParam>(TParam param, Func<TParam, CancellationToken, Task<TResult>> func, CancellationToken token)
        where TParam : struct =>
        _policy.ExecuteAsync(param, func, _handler, token);

    public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TParam>(TParam param, Func<TParam, CancellationToken, Task<TResult>> func, CancellationToken token)
        where TParam : struct =>
        _policy.ExecuteAndCaptureAsync(param, func, _handler, token);
}

public sealed class AsyncPolicyAndHandlerWrapper
{
    private readonly IAsyncPolicy _policy;
    private readonly Handlers _handler;

    public AsyncPolicyAndHandlerWrapper(IAsyncPolicy policy, Handlers handler)
    {
        _policy = policy;
        _handler = handler;
    }

    public Task ExecuteAsync<TParam>(TParam param, Func<TParam, CancellationToken, Task> func, CancellationToken token)
        where TParam : struct =>
        _policy.ExecuteAsync(param, func, _handler, token);
}
