using Azure.Messaging.EventHubs.Producer;

namespace Retry.Services;

public interface IEventHubProducerClientWrapper
{
    Task SendAsync(EventDataBatch eventBatch, CancellationToken cancellationToken = default);

    Task<EventDataBatch> CreateBatchAsync(CreateBatchOptions options, CancellationToken cancellationToken = default);
}