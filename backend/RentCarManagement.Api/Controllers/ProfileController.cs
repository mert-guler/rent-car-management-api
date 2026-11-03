using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCarManagement.Application.Abstractions;
using RentCarManagement.Application.Contracts;

namespace RentCarManagement.Api.Controllers;

[ApiController, Route("api/profile"), Authorize]
public sealed class ProfileController(IProfileApplicationService profiles) : ApiControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Get() => FromResult(await profiles.GetAsync(UserId));

    [HttpPut]
    public async Task<IActionResult> Update(UpdateProfileRequest request)
    {
        var result = await profiles.UpdateAsync(UserId, request);
        return result.Succeeded ? NoContent() : FromResult(result);
    }
}
