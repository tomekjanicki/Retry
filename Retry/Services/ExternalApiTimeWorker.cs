namespace Retry.Services;

public sealed class ExternalApiTimeWorker : BackgroundService
{
    private readonly IExternalApiClient _api;
    private readonly ILogger<ExternalApiTimeWorker> _logger;

    public ExternalApiTimeWorker(IExternalApiClient api, ILogger<ExternalApiTimeWorker> logger)
    {
        _api = api;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Start loop.");
                var result = await _api.GetTimeAsString(false, stoppingToken).ConfigureAwait(false);
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