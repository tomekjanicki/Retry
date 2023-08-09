namespace Retry.Services;

public sealed class InternalApiUserWorker : BackgroundService
{
    private readonly IInternalApiClient _api;
    private readonly ILogger<InternalApiUserWorker> _logger;

    public InternalApiUserWorker(IInternalApiClient api, ILogger<InternalApiUserWorker> logger)
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
                var result = await _api.GetUserFullNameById(id, true, stoppingToken).ConfigureAwait(false);
                result.Switch(s =>
                {
                    _logger.LogInformation("Returned: {FullName}.", s);
                }, _ =>
                {
                    _logger.LogInformation("User with {Id} not found.", id);
                }, error =>
                {
                    _logger.LogError("Error during fetching user with {Id} {Error}.", id, error);
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