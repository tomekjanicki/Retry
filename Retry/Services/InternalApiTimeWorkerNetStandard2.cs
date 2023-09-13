using ApiClient.Services;
using Polly.CircuitBreaker;
using Retry.Extensions;

namespace Retry.Services;

public sealed class InternalApiTimeWorkerNetStandard2 : BackgroundService
{
    private readonly IInternalApiClientNetStandard _api;
    private readonly ILogger<InternalApiTimeWorkerNetStandard2> _logger;

    public InternalApiTimeWorkerNetStandard2(IInternalApiClientNetStandard api, ILogger<InternalApiTimeWorkerNetStandard2> logger)
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
                using var cancellationTokenSource = Helper.CreateCancellationTokenSource(stoppingToken, TimeSpan.FromSeconds(2));
                _logger.LogInformation("Start loop.");
                await ExecuteLoop(500, cancellationTokenSource.Token).ConfigureAwait(false);
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
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ExecuteLoop(int delayInMilliseconds, CancellationToken token)
    {
        for (var i = 0; i < 5; i++)
        {
            var result = await _api.GetTimeAsString(false, delayInMilliseconds, token).ConfigureAwait(false);
            _logger.LogInformation("Returned: {Result}.", result);
            Thread.Sleep(2000);
            token.ThrowIfCancellationRequested();
        }
    }
}