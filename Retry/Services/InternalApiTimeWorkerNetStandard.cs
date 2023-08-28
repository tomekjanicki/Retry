using ApiClient.Services;
using Polly.CircuitBreaker;
using Retry.Extensions;

namespace Retry.Services;

public sealed class InternalApiTimeWorkerNetStandard : BackgroundService
{
    private readonly IInternalApiClientNetStandard _api;
    private readonly ILogger<InternalApiTimeWorkerNetStandard> _logger;

    public InternalApiTimeWorkerNetStandard(IInternalApiClientNetStandard api, ILogger<InternalApiTimeWorkerNetStandard> logger)
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
                _logger.LogError("HttpRequestException {StatusCode}.", e.GetHttpStatusCode());
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