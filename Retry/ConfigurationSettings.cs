﻿using Retry.Resiliency.Models;

namespace Retry;

public sealed class ConfigurationSettings
{
    public const string SectionName = "configurationSettings";

    public RetryAndCircuitBreakerPolicyConfiguration RetryAndCircuitBreakerPolicyConfiguration { get; init; } = new();

    public Uri ApiUri { get; init; } = new("http://localhost:5046");
}