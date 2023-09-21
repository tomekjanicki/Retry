namespace Retry.Services;

public static class Helper
{
    public static CancellationTokenSource CreateCancellationTokenSource(TimeSpan timeout, CancellationToken token) => 
        CancellationTokenSource.CreateLinkedTokenSource(token, new CancellationTokenSource(timeout).Token);
}