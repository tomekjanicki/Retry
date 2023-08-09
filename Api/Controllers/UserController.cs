using Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;

    private static readonly IReadOnlyCollection<User> Users = new[]
    {
        new User { Id = 1, FirstName = "firstName1", LastName = "lastName1" },
        new User { Id = 2, FirstName = "firstName2", LastName = "lastName2" }
    };

    public UserController(ILogger<UserController> logger) => 
        _logger = logger;

    [HttpGet]
    public IActionResult Get(int id, string? mode)
    {
        if (mode == "fail")
        {
            return this.GetInternalServerErrorWithLogging(_logger, nameof(Get));
        }
        var user = Users.SingleOrDefault(user => user.Id == id);

        return user is not null ? this.GetOkWithLogging(user, _logger, nameof(Get)) : this.GetNotFoundWithLogging(_logger, nameof(Get));
    }
}