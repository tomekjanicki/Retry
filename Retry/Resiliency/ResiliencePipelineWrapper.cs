using Polly;
using Retry.Resiliency.Models;

namespace Retry.Resiliency;

public sealed class ResiliencePipelineWrapper<TResult>
{
    private readonly ResiliencePipeline<TResult> _pipeline;
    private readonly ILogger _logger;

    public ResiliencePipelineWrapper(ResiliencePipeline<TResult> pipeline, ILogger logger)
    {
        _pipeline = pipeline;
        _logger = logger;
    }

    public async Task<TResult> ExecuteAsync<TParam>(TParam param, Func<TParam, CancellationToken, Task<TResult>> func,
        CancellationToken token) where TParam : struct =>
        await ExecuteAsyncWithValueTask((param, func), static async (p, token) => await p.func(p.param, token).ConfigureAwait(false), token)
            .ConfigureAwait(false);

    public async ValueTask<TResult> ExecuteAsyncWithValueTask<TParam>(TParam param, Func<TParam, CancellationToken, ValueTask<TResult>> func, CancellationToken token)
        where TParam : struct
    {
        using var contextWrapper = ResilienceContextWrapper.CreateWithLogger(_logger, token);

        return await _pipeline.ExecuteAsync(static (_, p) => p.func(p.param, p.token),
            contextWrapper.Context, (param, func, token)).ConfigureAwait(false);
    }

    public async Task<Outcome<TResult>> ExecuteOutcomeAsync<TParam>(TParam param,
        Func<TParam, CancellationToken, Task<TResult>> func, CancellationToken token) where TParam : struct =>
        await ExecuteOutcomeAsyncWithValueTask((param, func), static async (p, token) => await p.func(p.param, token).ConfigureAwait(false), token)
            .ConfigureAwait(false);

    public async ValueTask<Outcome<TResult>> ExecuteOutcomeAsyncWithValueTask<TParam>(TParam param, Func<TParam, CancellationToken, ValueTask<TResult>> func, CancellationToken token)
        where TParam : struct
    {
        using var contextWrapper = ResilienceContextWrapper.CreateWithLogger(_logger, token);

        return await _pipeline.ExecuteOutcomeAsync(static async (_, p) => Outcome.FromResult(await p.func(p.param, p.token)),
            contextWrapper.Context, (param, func, token)).ConfigureAwait(false);
    }
}

public sealed class ResiliencePipelineWrapper
{
    private readonly ResiliencePipeline _pipeline;
    private readonly ILogger _logger;

    public ResiliencePipelineWrapper(ResiliencePipeline pipeline, ILogger logger)
    {
        _pipeline = pipeline;
        _logger = logger;
    }

    public async Task ExecuteAsync<TParam>(TParam param, Func<TParam, CancellationToken, Task> func, CancellationToken token)
        where TParam : struct =>
        await ExecuteAsyncWithValueTask((param, func), static async (p, token) => await p.func(p.param, token).ConfigureAwait(false), token)
            .ConfigureAwait(false);

    public async ValueTask ExecuteAsyncWithValueTask<TParam>(TParam param, Func<TParam, CancellationToken, ValueTask> func, CancellationToken token)
        where TParam : struct
    {
        using var contextWrapper = ResilienceContextWrapper.CreateWithLogger(_logger, token);
        await _pipeline.ExecuteAsync(static (_, p) => p.func(p.param, p.token),
            contextWrapper.Context, (param, func, token)).ConfigureAwait(false);
    }
}