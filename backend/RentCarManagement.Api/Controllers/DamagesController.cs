using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCarManagement.Application.Abstractions;
using RentCarManagement.Application.Contracts;

namespace RentCarManagement.Api.Controllers;

[ApiController, Route("api/damages"), Authorize]
public sealed class DamagesController(IDamageApplicationService damages) : ApiControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("rentals/{rentalId:guid}")]
    public async Task<IActionResult> Report(Guid rentalId, DamageRequest request)
    {
        var result = await damages.ReportAsync(UserId, rentalId, request);
        return result.Succeeded ? Ok(result.Value) : FromResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetMine() => Ok(await damages.GetMineAsync(UserId));

    [HttpGet("admin"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllForAdmin() => Ok(await damages.GetAllForAdminAsync());

    [HttpPut("{id:guid}/assess"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Assess(Guid id, AssessDamageRequest request)
    {
        var result = await damages.AssessAsync(id, request);
        return result.Succeeded ? NoContent() : FromResult(result);
    }

    [HttpPost("{id:guid}/pay")]
    public async Task<IActionResult> Pay(Guid id)
    {
        var result = await damages.PayAsync(UserId, id);
        return result.Succeeded ? Ok(result.Value) : FromResult(result);
    }
}
