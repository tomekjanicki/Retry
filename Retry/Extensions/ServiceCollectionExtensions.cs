using Microsoft.Extensions.Options;

namespace Retry.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWithRetry<TInterface, TImplementation>(this IServiceCollection serviceCollection, Func<TImplementation, IOptions<ConfigurationSettings>, TInterface> func)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        serviceCollection.AddSingleton<TImplementation>();
        serviceCollection.AddSingleton(provider => func(provider.GetRequiredService<TImplementation>(), provider.GetRequiredService<IOptions<ConfigurationSettings>>()));

        return serviceCollection;
    }
}