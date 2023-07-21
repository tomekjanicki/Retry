using Polly;
using Retry.Extensions;

namespace Retry;

public sealed class PolicyAndHandlerWrapper<TResult>
{
    private readonly IAsyncPolicy<TResult> _policy;
    private readonly Handlers<TResult> _handler;

    public PolicyAndHandlerWrapper(IAsyncPolicy<TResult> policy, Handlers<TResult> handler)
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