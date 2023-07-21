using Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class UserController : ControllerBase
{
    private static readonly IReadOnlyCollection<User> Users = new[]
    {
        new User { Id = 1, FirstName = "firstName1", LastName = "lastName1" },
        new User { Id = 2, FirstName = "firstName2", LastName = "lastName2" }
    };

    [HttpGet]
    public IActionResult Get(int id, string? mode)
    {
        if (mode == "fail")
        {
            throw new InvalidOperationException();
        }
        var user = Users.SingleOrDefault(user => user.Id == id);

        return user is not null ? Ok(user) : NotFound();
    }
}