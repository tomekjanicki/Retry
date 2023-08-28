using ApiClient.Services;
using Microsoft.Extensions.Options;
using Retry.Extensions;
using Retry.Resiliency;
using Retry.Resiliency.Models;
using Retry.Services;

namespace Retry;

public static class ConfigureIoC
{
    public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<PolicyAndHandlerWrapperProvider>();
        services.AddOptions<ConfigurationSettings>().Bind(context.Configuration.GetSection(ConfigurationSettings.SectionName));
        //services.AddHostedService<ExternalApiTimeWorkerNetStandard>();
        //services.AddHostedService<ExternalApiTimeWorker>();
        //services.AddHostedService<ExternalApiUserWorker>();
        services.AddWithPolicyWrapper<IExternalApiClient, ExternalApiClient, WithRetryAndCircuitBreakerExternalApiClient>(static (api, provider, logger) => new WithRetryAndCircuitBreakerExternalApiClient(api, provider, logger));
        services.AddWithPolicyWrapper<IExternalApiClientNetStandard, ExternalApiClientNetStandard, WithRetryAndCircuitBreakerExternalApiClientNetStandard>(static (api, provider, logger) => new WithRetryAndCircuitBreakerExternalApiClientNetStandard(api, provider, logger));
        services.AddHttpClient(ExternalApiClient.Name, static (provider, client) => ConfigureClient(provider, client));
        services.AddHttpClient(ExternalApiClientNetStandard.Name, static (provider, client) => ConfigureClient(provider, client));
        //services.AddHostedService<InternalApiTimeWorker>();
        //services.AddHostedService<InternalApiUserWorker>();
        services.AddHostedService<InternalApiTimeWorkerNetStandard>();
        services.AddSingleton<IInternalApiClientNetStandard, InternalApiClientNetStandard>();
        services.AddSingleton<PolicyExecutionContextDelegatingHandler>();
        services.AddSingleton<IInternalApiClient, InternalApiClient>();
        services.AddHttpClient(InternalApiClientNetStandard.Name, static (provider, client) => ConfigureClient(provider, client))
            .AddHttpMessageHandler(static provider => provider.GetRequiredService<PolicyExecutionContextDelegatingHandler>())
            .AddPolicyHandler(static (provider, message) => HttpClientResiliencyHelper.GetSingleInstanceOfRetryAndCircuitBreakerAsyncPolicy(provider.GetRequiredService<IOptions<RetryAndCircuitBreakerPolicyConfiguration>>().Value, message));
        services.AddHttpClient(InternalApiClient.Name, static (provider, client) => ConfigureClient(provider, client))
            .AddHttpMessageHandler(static provider => provider.GetRequiredService<PolicyExecutionContextDelegatingHandler>())
            .AddPolicyHandler(static (provider, message) => HttpClientResiliencyHelper.GetSingleInstanceOfRetryAndCircuitBreakerAsyncPolicy(provider.GetRequiredService<IOptions<RetryAndCircuitBreakerPolicyConfiguration>>().Value, message));
    }

    private static void ConfigureClient(IServiceProvider provider, HttpClient client) => 
        client.BaseAddress = provider.GetRequiredService<IOptions<ConfigurationSettings>>().Value.ApiUri;
}