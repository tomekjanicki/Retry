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
                using var cts = CancellationTokenSources.Create(TimeSpan.FromSeconds(2), stoppingToken);
                _logger.LogInformation("Start loop.");
                var result = await _api.GetTimeAsString(false, 5000, cts.Token).ConfigureAwait(false);
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
            catch (TaskCanceledException e)
            {
                _logger.LogError("TaskCanceledException. Inner: {Inner}", e.InnerException);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception in loop.");
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}