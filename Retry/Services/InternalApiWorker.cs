namespace Retry.Services;

public sealed class InternalApiWorker : BackgroundService
{
    private readonly IInternalApiClient _internalApiClient;
    private readonly ILogger<InternalApiWorker> _logger;

    public InternalApiWorker(IInternalApiClient internalApiClient, ILogger<InternalApiWorker> logger)
    {
        _internalApiClient = internalApiClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Start loop.");
                var result = await _internalApiClient.GetTimeAsString(true, stoppingToken).ConfigureAwait(false);
                _logger.LogInformation("Returned: {Result}.", result);
                _logger.LogInformation("End loop.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception in loop.");
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}