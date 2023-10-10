using Retry.Resiliency;

namespace Retry.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWithResiliencePipelineWrapper<TInterface, TImplementation, TResiliencePipelineWrapper>(this IServiceCollection serviceCollection, Func<TImplementation, ResiliencePipelineWrapperProvider, ILogger<TResiliencePipelineWrapper>, TInterface> func)
        where TInterface : class
        where TImplementation : class, TInterface
        where TResiliencePipelineWrapper : class, TInterface
    {
        serviceCollection.AddSingleton<TImplementation>();
        serviceCollection.AddSingleton(provider => func(provider.GetRequiredService<TImplementation>(), provider.GetRequiredService<ResiliencePipelineWrapperProvider>(), provider.GetRequiredService<ILogger<TResiliencePipelineWrapper>>()));

        return serviceCollection;
    }
}