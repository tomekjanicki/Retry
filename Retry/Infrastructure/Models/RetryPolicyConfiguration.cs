namespace Retry.Infrastructure.Models;

public sealed class RetryPolicyConfiguration
{
    public TimeSpan FirstRetryDelay { get; init; } = TimeSpan.FromSeconds(1);
    public int RetryCount { get; init; } = 2;
}