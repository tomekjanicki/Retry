using Microsoft.Extensions.Options;
using Retry.Extensions;
using Retry.Services;

namespace Retry;

public static class ConfigureIoC
{
    public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddOptions<ConfigurationSettings>().Bind(context.Configuration.GetSection(ConfigurationSettings.SectionName));
        //services.AddHostedService<ExternalApiTimeWorker>();
        services.AddHostedService<ExternalApiUserWorker>();
        services.AddWithRetry<IExternalApiClient, ExternalApiClient, WithRetryExternalApiClient>(static (api, options, logger) => new WithRetryExternalApiClient(api, options, logger));
        services.AddHttpClient(ExternalApiClient.Name, static (provider, client) => ConfigureClient(provider, client));
        //services.AddHostedService<InternalApiWorker>();
        //services.AddSingleton<IInternalApiClient, InternalApiClient>();
        //services.AddHttpClient(InternalApiClient.Name, static (provider, client) => ConfigureClient(provider, client));
    }

    private static void ConfigureClient(IServiceProvider provider, HttpClient client)
    {
        var settings = provider.GetRequiredService<IOptions<ConfigurationSettings>>().Value;
        client.BaseAddress = settings.ApiUri;
    }
}