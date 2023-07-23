using Polly;

namespace Retry.Infrastructure.Models;

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

public sealed class Handlers
{
    public Handlers(ILogger logger, Action<Exception, ILogger> onBrake, Action<Exception, ILogger> onRetry)
    {
        Logger = logger;
        OnBrake = onBrake;
        OnRetry = onRetry;
    }

    public ILogger Logger { get; }

    public Action<Exception, ILogger> OnBrake { get; }

    public Action<Exception, ILogger> OnRetry { get; }
}