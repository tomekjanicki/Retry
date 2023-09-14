using ApiClient.Services;
using Polly.CircuitBreaker;
using Retry.Extensions;

namespace Retry.Services;

public sealed class InternalApiTimeWorkerNetStandard3 : BackgroundService
{
    private readonly IInternalApiClientNetStandard _api;
    private readonly ILogger<InternalApiTimeWorkerNetStandard3> _logger;

    public InternalApiTimeWorkerNetStandard3(IInternalApiClientNetStandard api, ILogger<InternalApiTimeWorkerNetStandard3> logger)
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
                try
                {
                    await Execute(3000).WaitAsync(TimeSpan.FromSeconds(2), stoppingToken).ConfigureAwait(false);
                    _logger.LogInformation("Within time.");
                }
                catch (TimeoutException)
                {
                    _logger.LogWarning("Time outed.");
                }
                _logger.LogInformation("End loop.");
            }
            catch (HttpRequestException e) when (e.ShouldHandleTransientHttpRequestException())
            {
                _logger.LogError("Transient HttpRequestException {StatusCode}.", e.GetHttpStatusCode());
            }
            catch (BrokenCircuitException)
            {
                _logger.LogError("BrokenCircuitException.");
            }
            catch (OperationCanceledException e)
            {
                _logger.LogError("OperationCanceledException. Inner exception: {Inner}", e.InnerException);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception in loop.");
            }
            await Task.Delay(5000, stoppingToken);
        }
    }

    private async Task Execute(int delayInMilliseconds)
    {
        var result = await _api.GetTimeAsString(false, delayInMilliseconds, CancellationToken.None).ConfigureAwait(false);
        _logger.LogInformation("Returned: {Result}.", result);
    }
}