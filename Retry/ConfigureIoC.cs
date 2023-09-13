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
        services.AddHttpClient(ExternalApiClient.Name);
        services.AddHttpClient(ExternalApiClientNetStandard.Name);
        //services.AddHostedService<InternalApiTimeWorker>();
        //services.AddHostedService<InternalApiUserWorker>();
        //services.AddHostedService<InternalApiTimeWorkerNetStandard>();
        services.AddHostedService<InternalApiTimeWorkerNetStandard2>();
        services.AddSingleton<IInternalApiClientNetStandard, InternalApiClientNetStandard>();
        services.AddTransient<PolicyExecutionContextDelegatingHandler>();
        services.AddSingleton<IInternalApiClient, InternalApiClient>();
        services.AddHttpClient(InternalApiClientNetStandard.Name).AddHandlers();
        services.AddHttpClient(InternalApiClient.Name).AddHandlers();
    }

    private static IHttpClientBuilder AddHttpClient(this IServiceCollection services, string name) =>
        services.AddHttpClient(name, static (provider, client) => ConfigureClient(provider, client));

    private static void AddHandlers(this IHttpClientBuilder clientBuilder) =>
        clientBuilder
            .AddHttpMessageHandler(static provider => provider.GetRequiredService<PolicyExecutionContextDelegatingHandler>())
            .AddPolicyHandler(static (provider, message) => HttpClientResiliencyHelper.GetSingleInstanceOfRetryAndCircuitBreakerAsyncPolicy(provider.GetRequiredService<IOptions<RetryAndCircuitBreakerPolicyConfiguration>>().Value, message));

    private static void ConfigureClient(IServiceProvider provider, HttpClient client) => 
        client.BaseAddress = provider.GetRequiredService<IOptions<ConfigurationSettings>>().Value.ApiUri;
}