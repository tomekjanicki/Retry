using Retry;

var host = Host
    .CreateDefaultBuilder(args)
    .ConfigureServices(static (context, services) => ConfigureIoC.ConfigureServices(context, services))
    .Build();

await host.RunAsync();