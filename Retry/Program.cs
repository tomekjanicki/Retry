using Microsoft.Extensions.Options;
using Retry;
using Retry.Extensions;
using Retry.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(static (context, services) =>
    {
        services.AddOptions<ConfigurationSettings>().Bind(context.Configuration.GetSection(ConfigurationSettings.SectionName));
        //services.AddHostedService<ExternalApiTimeWorker>();
        services.AddHostedService<ExternalApiUserWorker>();
        services.AddWithRetry<IExternalApiClient, ExternalApiClient, WithRetryExternalApiClient>(static (api, options, logger) => new WithRetryExternalApiClient(api, options, logger));
        services.AddHttpClient(ExternalApiClient.Name, static (provider, client) => ConfigureClient(provider, client));
        //services.AddHostedService<InternalApiWorker>();
        //services.AddSingleton<IInternalApiClient, InternalApiClient>();
        //services.AddHttpClient(InternalApiClient.Name, static (provider, client) => ConfigureClient(provider, client));
    })
    .Build();

await host.RunAsync();

static void ConfigureClient(IServiceProvider provider, HttpClient client)
{
    var settings = provider.GetRequiredService<IOptions<ConfigurationSettings>>().Value;
    client.BaseAddress = settings.ApiUri;
}
