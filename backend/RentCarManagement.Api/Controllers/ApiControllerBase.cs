using Microsoft.AspNetCore.Mvc;
using RentCarManagement.Application.Contracts;

namespace RentCarManagement.Api.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult FromResult<T>(OperationResult<T> result)
    {
        if (result.Error is { } error)
            return StatusCode(error.HttpStatusCode, new { code = error.Code, message = error.Message });

        return Ok(result.Value);
    }
}
