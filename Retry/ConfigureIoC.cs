using Microsoft.Extensions.Options;
using Retry.Extensions;
using Retry.Resiliency;
using Retry.Services;

namespace Retry;

public static class ConfigureIoC
{
    public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<PolicyAndHandlerWrapperProvider>();
        services.AddOptions<ConfigurationSettings>().Bind(context.Configuration.GetSection(ConfigurationSettings.SectionName));
        //services.AddHostedService<ExternalApiTimeWorker>();
        //services.AddHostedService<ExternalApiUserWorker>();
        services.AddWithPolicyWrapper<IExternalApiClient, ExternalApiClient, WithRetryAndCircuitBreakerExternalApiClient>(static (api, provider, logger) => new WithRetryAndCircuitBreakerExternalApiClient(api, provider, logger));
        services.AddHttpClient(ExternalApiClient.Name, static (provider, client) => ConfigureClient(provider, client));
        services.AddHostedService<InternalApiWorker>();
        services.AddSingleton<IInternalApiClient, InternalApiClient>();
        services.AddHttpClient(InternalApiClient.Name, static (provider, client) => ConfigureClient(provider, client))
            .AddPolicyHandler(static (provider, _) => HttpClientResiliencyHelper.GetRetryAndCircuitBreakerAsyncPolicy(provider.GetRequiredService<IOptions<ConfigurationSettings>>().Value.RetryAndCircuitBreakerPolicyConfiguration));
    }

    private static void ConfigureClient(IServiceProvider provider, HttpClient client) => 
        client.BaseAddress = provider.GetRequiredService<IOptions<ConfigurationSettings>>().Value.ApiUri;
}