using ApiClient.Services;
using Microsoft.Extensions.Options;
using Retry.Extensions;
using Retry.Resiliency;
using Retry.Services;

namespace Retry;

public static class ConfigureIoC
{
    public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<ResiliencePipelineWrapperProvider>();
        services.AddOptions<ConfigurationSettings>().Bind(context.Configuration.GetSection(ConfigurationSettings.SectionName));
#pragma warning disable S125
        //services.AddHostedService<ExternalApiTimeWorkerNetStandard>();
        //services.AddHostedService<ExternalApiTimeWorker>();
        //services.AddHostedService<ExternalApiUserWorker>();
#pragma warning restore S125
        services.AddWithResiliencePipelineWrapper<IExternalApiClient, ExternalApiClient, WithRetryAndCircuitBreakerExternalApiClient>(static (api, provider, logger) => new WithRetryAndCircuitBreakerExternalApiClient(api, provider, logger));
        services.AddWithResiliencePipelineWrapper<IExternalApiClientNetStandard, ExternalApiClientNetStandard, WithRetryAndCircuitBreakerExternalApiClientNetStandard>(static (api, provider, logger) => new WithRetryAndCircuitBreakerExternalApiClientNetStandard(api, provider, logger));
        services.AddHttpClient(ExternalApiClient.Name);
        services.AddHttpClient(ExternalApiClientNetStandard.Name);
#pragma warning disable S125
        //services.AddHostedService<InternalApiTimeWorker>();
        //services.AddHostedService<InternalApiUserWorker>();
        //services.AddHostedService<InternalApiTimeWorkerNetStandard2>();
        //services.AddHostedService<InternalApiTimeWorkerNetStandard3>();
        //services.AddHostedService<InternalApiTimeWorkerNetStandard4>();
#pragma warning restore S125
        services.AddHostedService<InternalApiTimeWorkerNetStandard>();
        services.AddSingleton<IInternalApiClientNetStandard, InternalApiClientNetStandard>();
        services.AddSingleton<IInternalApiClient, InternalApiClient>();
        services.AddHttpClient(InternalApiClientNetStandard.Name).AddWithLoggingDelegatingHandler();
        services.AddHttpClient(InternalApiClient.Name).AddWithLoggingDelegatingHandler();
    }

    private static IHttpClientBuilder AddHttpClient(this IServiceCollection services, string name) =>
        services.AddHttpClient(name, static (provider, client) => ConfigureClient(provider, client));

    private static void ConfigureClient(IServiceProvider provider, HttpClient client) => 
        client.BaseAddress = provider.GetRequiredService<IOptions<ConfigurationSettings>>().Value.ApiUri;
}
