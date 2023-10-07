using Polly;

namespace Retry.Resiliency.Models;

public sealed class ResilienceContextWrapper : IDisposable
{
    public ResilienceContextWrapper(CancellationToken token) =>
        Context = ResilienceContextPool.Shared.Get(token);

    public ResilienceContext Context { get; }

    public void Dispose() =>
        ResilienceContextPool.Shared.Return(Context);

    public static ResilienceContextWrapper CreateWithLogger(ILogger logger, CancellationToken token)
    {
        var wrapper = new ResilienceContextWrapper(token);
        wrapper.Context.Properties.Set(GenericResiliencePipelines.Key, logger);

        return wrapper;
    }
}