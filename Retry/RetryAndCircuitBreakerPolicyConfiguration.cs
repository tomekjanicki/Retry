namespace Retry;

public sealed class RetryAndCircuitBreakerPolicyConfiguration
{
    public TimeSpan FirstRetryDelay { get; init; } = TimeSpan.FromSeconds(1);
    public int RetryCount { get; init; } = 2;
    public double FailureThreshold { get; init; } = 0.5;
    public TimeSpan SamplingDuration { get; init; } = TimeSpan.FromSeconds(10);
    public int MinimumThroughput { get; init; } = 4;
    public TimeSpan BreakDuration { get; init; } = TimeSpan.FromSeconds(10);

}