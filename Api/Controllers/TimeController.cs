using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class TimeController : ControllerBase
{
    private readonly ILogger<TimeController> _logger;

    public TimeController(ILogger<TimeController> logger) => _logger = logger;


    [HttpGet]
    public async Task<IActionResult> Get(string? mode, int? delay, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting get request.");
            if (delay is not null)
            {
                await Task.Delay(delay.Value, cancellationToken).ConfigureAwait(false);
            }

            return mode != "fail" ? this.GetOkWithLogging(DateTime.Now.ToLongTimeString(), _logger, nameof(Get)) : this.GetInternalServerErrorWithLogging(_logger, nameof(Get));
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Task was cancelled");

            return this.GetTimeoutWithLogging(_logger, nameof(Get));
        }
    }
}