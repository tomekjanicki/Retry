﻿namespace Retry.Services;

public sealed class CancellationTokenSources : IDisposable
{
    private CancellationTokenSources(CancellationTokenSource linkedTokenSource, CancellationTokenSource internalTokenSource)
    {
        _linkedTokenSource = linkedTokenSource;
        _internalTokenSource = internalTokenSource;
    }

    private readonly CancellationTokenSource _linkedTokenSource;
    private readonly CancellationTokenSource _internalTokenSource;

    public static CancellationTokenSources Create(TimeSpan timeout, CancellationToken cancellationToken)
    {
        var internalTokenSource = new CancellationTokenSource(timeout);
        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, internalTokenSource.Token);

        return new CancellationTokenSources(linkedTokenSource, internalTokenSource);
    }

    public CancellationToken Token => _linkedTokenSource.Token;

    public void Dispose()
    {
        _linkedTokenSource.Dispose();
        _internalTokenSource.Dispose();
    }
}