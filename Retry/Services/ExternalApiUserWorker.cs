namespace Retry.Services;

public sealed class ExternalApiUserWorker : BackgroundService
{
    private readonly IExternalApiClient _api;
    private readonly ILogger<ExternalApiUserWorker> _logger;

    public ExternalApiUserWorker(IExternalApiClient api, ILogger<ExternalApiUserWorker> logger)
    {
        _api = api;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const int id = 1;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Start loop.");
                var result = await _api.GetUserFullNameById(id, false, stoppingToken).ConfigureAwait(false);
                result.Switch(s =>
                {
                    _logger.LogInformation("Returned: {FullName}.", s);
                }, _ =>
                {
                    _logger.LogInformation("User with {Id} not found.", id);
                }, error =>
                {
                    _logger.LogError("Error during fetching user with {Id} {Status} {Details}.", id, error.StatusCode, error.Message);
                });
                
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