using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class DataController : ControllerBase
{
    [HttpGet]
    public IActionResult Get(string? mode)
    {
        if (mode != "fail")
        {
            return Ok(new List<string> { "1", "2" });
        }

        throw new InvalidOperationException();
    }
}