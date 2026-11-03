using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCarManagement.Application.Abstractions;
using RentCarManagement.Application.Contracts;
using RentCarManagement.Domain.Entities;

namespace RentCarManagement.Api.Controllers;

[ApiController, Route("api/rentals"), Authorize]
public sealed class RentalsController(IRentalApplicationService rentals) : ApiControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Rental>>> History() => Ok(await rentals.GetHistoryAsync(UserId));

    [HttpGet("admin"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminActiveRentals() => Ok(await rentals.GetAdminActiveAsync());

    [HttpPost("admin/{id:guid}/return"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminReturn(Guid id, ReturnRentalRequest request)
    {
        var result = await rentals.AdminReturnAsync(id, request);
        return result.Succeeded ? Ok(result.Value) : FromResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(RentalRequest request)
    {
        var result = await rentals.CreateAsync(UserId, request);
        return result.Succeeded ? Ok(result.Value) : FromResult(result);
    }

    [HttpPut("{id:guid}/extend")]
    public async Task<IActionResult> Extend(Guid id, ExtendRentalRequest request)
    {
        var result = await rentals.ExtendAsync(UserId, id, request);
        return result.Succeeded ? Ok(result.Value) : FromResult(result);
    }

    [HttpPost("{id:guid}/return")]
    public async Task<IActionResult> Return(Guid id, ReturnRentalRequest request)
    {
        var result = await rentals.ReturnAsync(UserId, id, request);
        return result.Succeeded ? Ok(result.Value) : FromResult(result);
    }
}
