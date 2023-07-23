using Retry.Infrastructure;

namespace Retry.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWithRetry<TInterface, TImplementation, TDecorator>(this IServiceCollection serviceCollection, Func<TImplementation, PolicyAndHandlerWrapperProvider, ILogger<TDecorator>, TInterface> func)
        where TInterface : class
        where TImplementation : class, TInterface
        where TDecorator : class, TInterface
    {
        serviceCollection.AddSingleton<TImplementation>();
        serviceCollection.AddSingleton(provider => func(provider.GetRequiredService<TImplementation>(), provider.GetRequiredService<PolicyAndHandlerWrapperProvider>(), provider.GetRequiredService<ILogger<TDecorator>>()));

        return serviceCollection;
    }
}