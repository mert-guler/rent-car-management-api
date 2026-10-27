using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
namespace RentCarManagement.Api.Controllers;
[ApiController, Route("error")]
public sealed class ErrorController(ILogger<ErrorController> logger) : ControllerBase
{
    [HttpGet, HttpPost, HttpPut, HttpDelete] public IActionResult Error() { var exception = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error; logger.LogError(exception, "Unhandled API exception"); return Problem("An unexpected error occurred."); }
}
