using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class DataController : ControllerBase
{
    private readonly ILogger<DataController> _logger;

    public DataController(ILogger<DataController> logger) => 
        _logger = logger;

    [HttpGet]
    public IActionResult Get(string? mode)
    {
        _logger.LogInformation("Starting get request.");

        return mode != "fail" ? this.GetOkWithLogging(new List<string> { "1", "2" }, _logger, nameof(Get)) : this.GetInternalServerErrorWithLogging(_logger, nameof(Get));
    }
}