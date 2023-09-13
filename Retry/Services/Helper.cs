namespace Retry.Services;

public static class Helper
{
    public static CancellationTokenSource CreateCancellationTokenSource(CancellationToken token, TimeSpan timeout) => 
        CancellationTokenSource.CreateLinkedTokenSource(token, new CancellationTokenSource(timeout).Token);
}