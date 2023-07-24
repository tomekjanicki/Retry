using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Retry.Resiliency;

namespace Retry.Services;

public sealed class WithCircuitBreakerEventHubProducerClientWrapper : IEventHubProducerClientWrapper
{
    private readonly IEventHubProducerClientWrapper _client;
    private readonly AsyncPolicyAndHandlerWrapper<EventDataBatch> _createBatchPolicyAndHandler;
    private readonly AsyncPolicyAndHandlerWrapper _sendAsyncAsyncPolicyAndHandler;

    public WithCircuitBreakerEventHubProducerClientWrapper(IEventHubProducerClientWrapper client, PolicyAndHandlerWrapperProvider provider, ILogger<WithCircuitBreakerEventHubProducerClientWrapper> logger)
    {
        _client = client;
        _createBatchPolicyAndHandler = provider.GetCircuitBreakerExceptionAsyncPolicyAndHandler<EventDataBatch, EventHubsException>
            (logger, static exception => exception.IsTransient);
        _sendAsyncAsyncPolicyAndHandler = provider.GetCircuitBreakerExceptionAsyncPolicyAndHandler<EventHubsException>
            (logger, static exception => exception.IsTransient);
    }

    public Task SendAsync(EventDataBatch eventBatch, CancellationToken cancellationToken = default) => 
        _sendAsyncAsyncPolicyAndHandler.ExecuteAsync((_client, eventBatch), static (p, token) => p._client.SendAsync(p.eventBatch, token), cancellationToken);

    public Task<EventDataBatch> CreateBatchAsync(CreateBatchOptions options, CancellationToken cancellationToken = default) => 
        _createBatchPolicyAndHandler.ExecuteAsync((_client, options), static (p, token) => p._client.CreateBatchAsync(p.options, token), cancellationToken);
}