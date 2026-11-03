using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentCarManagement.Application.Abstractions;
using RentCarManagement.Application.Contracts;
using RentCarManagement.Domain.Entities;

namespace RentCarManagement.Api.Controllers;

[ApiController, Route("api/vehicles")]
public sealed class VehiclesController(IVehicleApplicationService vehicles) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Vehicle>>> GetAll([FromQuery] VehicleStatus? status) =>
        Ok(await vehicles.GetAllAsync(status));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id) => FromResult(await vehicles.GetAsync(id));

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(VehicleRequest request)
    {
        var vehicle = await vehicles.CreateAsync(request);
        return CreatedAtAction(nameof(Get), new { id = vehicle.Id }, vehicle);
    }

    [HttpPut("{id:guid}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, VehicleRequest request)
    {
        var result = await vehicles.UpdateAsync(id, request);
        return result.Succeeded ? NoContent() : FromResult(result);
    }

    [HttpDelete("{id:guid}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await vehicles.DeactivateAsync(id);
        return result.Succeeded ? NoContent() : FromResult(result);
    }

    [HttpPost("{id:guid}/maintenance"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> CompleteMaintenance(Guid id)
    {
        var result = await vehicles.CompleteMaintenanceAsync(id);
        return result.Succeeded ? Ok(result.Value) : FromResult(result);
    }
}
