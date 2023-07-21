using Polly;

namespace Retry;

public sealed class Handlers<TResult>
{
    public Handlers(ILogger logger, Action<DelegateResult<TResult>, ILogger> onBrake, Action<DelegateResult<TResult>, ILogger> onRetry)
    {
        Logger = logger;
        OnBrake = onBrake;
        OnRetry = onRetry;
    }

    public ILogger Logger { get; }

    public Action<DelegateResult<TResult>, ILogger> OnBrake { get; }

    public Action<DelegateResult<TResult>, ILogger> OnRetry { get; }
}