using Retry.Resiliency;

namespace Retry.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWithPolicyWrapper<TInterface, TImplementation, TPolicyWrapper>(this IServiceCollection serviceCollection, Func<TImplementation, ResiliencePipelineWrapperProvider, ILogger<TPolicyWrapper>, TInterface> func)
        where TInterface : class
        where TImplementation : class, TInterface
        where TPolicyWrapper : class, TInterface
    {
        serviceCollection.AddSingleton<TImplementation>();
        serviceCollection.AddSingleton(provider => func(provider.GetRequiredService<TImplementation>(), provider.GetRequiredService<ResiliencePipelineWrapperProvider>(), provider.GetRequiredService<ILogger<TPolicyWrapper>>()));

        return serviceCollection;
    }
}