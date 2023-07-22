namespace Retry;

public sealed class CircuitBreakerPolicyConfiguration
{
    public double FailureThreshold { get; init; } = 0.5;
    public TimeSpan SamplingDuration { get; init; } = TimeSpan.FromSeconds(10);
    public int MinimumThroughput { get; init; } = 4;
    public TimeSpan BreakDuration { get; init; } = TimeSpan.FromSeconds(10);
}