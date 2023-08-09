using Polly.CircuitBreaker;
using Retry.Extensions;

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
                var result = await _api.GetTimeAsString(true, stoppingToken).ConfigureAwait(false);
                _logger.LogInformation("Returned: {Result}.", result);
                _logger.LogInformation("End loop.");
            }
            catch (HttpRequestException e) when (e.ShouldHandleTransientHttpRequestException())
            {
                _logger.LogError("HttpRequestException {StatusCode}.", e.StatusCode);
            }
            catch (BrokenCircuitException)
            {
                _logger.LogError("BrokenCircuitException.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception in loop.");
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}