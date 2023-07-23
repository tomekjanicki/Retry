using Azure.Messaging.EventHubs.Producer;

namespace Retry.Services;

public sealed class EventHubProducerClientWrapper : IEventHubProducerClientWrapper
{
    private readonly EventHubProducerClient _client;

    public EventHubProducerClientWrapper(EventHubProducerClient client) => _client = client;

    public Task SendAsync(EventDataBatch eventBatch, CancellationToken cancellationToken = default) => 
        _client.SendAsync(eventBatch, cancellationToken);

    public Task<EventDataBatch> CreateBatchAsync(CreateBatchOptions options, CancellationToken cancellationToken = default) => 
        _client.CreateBatchAsync(options, cancellationToken).AsTask();
}