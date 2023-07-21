using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class TimeController : ControllerBase
{
    [HttpGet]
    public IActionResult Get(string? mode)
    {
        if (mode != "fail")
        {
            return Ok(DateTime.Now.ToLongTimeString());
        }

        throw new InvalidOperationException();
    }
}