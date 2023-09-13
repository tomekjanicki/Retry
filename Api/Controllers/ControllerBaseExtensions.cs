using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public static class ControllerBaseExtensions
{
    public static IActionResult GetOkWithLogging<T>(this ControllerBase controllerBase, T obj, ILogger logger, string method)
    {
        logger.LogInformation("Returning Ok from {Method}.", method);

        return controllerBase.Ok(obj);
    }

    public static IActionResult GetNotFoundWithLogging(this ControllerBase controllerBase, ILogger logger, string method)
    {
        logger.LogInformation("Returning NotFound from {Method}.", method);

        return controllerBase.NotFound();
    }

    public static IActionResult GetInternalServerErrorWithLogging(this ControllerBase controllerBase, ILogger logger, string method)
    {
        logger.LogError("Returning InternalServerError from {Method}.", method);

        return controllerBase.StatusCode(500, "Error");
    }

    public static IActionResult GetTimeoutWithLogging(this ControllerBase controllerBase, ILogger logger, string method)
    {
        logger.LogError("Returning timeout from {Method}.", method);

        return controllerBase.StatusCode(408, "Error");
    }

}