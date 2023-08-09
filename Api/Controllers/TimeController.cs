using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class TimeController : ControllerBase
{
    private readonly ILogger<TimeController> _logger;

    public TimeController(ILogger<TimeController> logger) => _logger = logger;


    [HttpGet]
    public IActionResult Get(string? mode)
    {
        _logger.LogInformation("Starting get request.");

        return mode != "fail" ? this.GetOkWithLogging(DateTime.Now.ToLongTimeString(), _logger, nameof(Get)) : this.GetInternalServerErrorWithLogging(_logger, nameof(Get));
    }
}