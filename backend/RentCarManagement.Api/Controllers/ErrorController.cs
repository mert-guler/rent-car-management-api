using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RentCarManagement.Domain.Errors;
namespace RentCarManagement.Api.Controllers;
[ApiController, Route("error")]
public sealed class ErrorController(ILogger<ErrorController> logger) : ControllerBase
{
[HttpGet, HttpPost, HttpPut, HttpDelete] public IActionResult Error() { var exception = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error; logger.LogError(exception, "Unhandled API exception"); var problem = new ProblemDetails { Status = StatusCodes.Status500InternalServerError, Title = DomainErrorCodes.Unexpected.Message }; problem.Extensions["code"] = DomainErrorCodes.Unexpected.Code; return StatusCode(problem.Status.Value, problem); }
}
