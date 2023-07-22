namespace Retry;

public sealed class RetryAndCircuitBreakerPolicyConfiguration
{
    public RetryPolicyConfiguration RetryPolicy { get; init; } = new();

    public CircuitBreakerPolicyConfiguration CircuitBreakerPolicy { get; init; } = new();
}