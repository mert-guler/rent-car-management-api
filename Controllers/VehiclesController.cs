using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentCarManagement.Api.Data;
using RentCarManagement.Api.Models;
namespace RentCarManagement.Api.Controllers;
[ApiController, Route("api/vehicles")]
public sealed class VehiclesController(RentCarDbContext db) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<IEnumerable<Vehicle>>> GetAll([FromQuery] VehicleStatus? status) => Ok(await db.Vehicles.AsNoTracking().Where(x => status == null || x.Status == status).ToListAsync());
    [HttpGet("{id:int}")] public async Task<ActionResult<Vehicle>> Get(int id) { var item = await db.Vehicles.FindAsync(id); return item is null ? NotFound() : Ok(item); }
    [HttpPost, Authorize(Roles = "Admin")] public async Task<ActionResult<Vehicle>> Create(VehicleRequest request) { var vehicle = new Vehicle { Brand = request.Brand, Model = request.Model, Year = request.Year, Mileage = request.Mileage, MaintenanceMileageThreshold = request.MaintenanceMileageThreshold, DailyRate = request.DailyRate, ExtraKilometerRate = request.ExtraKilometerRate, Status = request.Status }; db.Vehicles.Add(vehicle); await db.SaveChangesAsync(); return CreatedAtAction(nameof(Get), new { id = vehicle.Id }, vehicle); }
    [HttpPut("{id:int}"), Authorize(Roles = "Admin")] public async Task<IActionResult> Update(int id, VehicleRequest request) { var v = await db.Vehicles.FindAsync(id); if (v is null) return NotFound(); v.Brand = request.Brand; v.Model = request.Model; v.Year = request.Year; v.Mileage = request.Mileage; v.MaintenanceMileageThreshold = request.MaintenanceMileageThreshold; v.DailyRate = request.DailyRate; v.ExtraKilometerRate = request.ExtraKilometerRate; v.Status = request.Status; await db.SaveChangesAsync(); return NoContent(); }
    [HttpDelete("{id:int}"), Authorize(Roles = "Admin")] public async Task<IActionResult> Delete(int id) { var v = await db.Vehicles.FindAsync(id); if (v is null) return NotFound(); v.Status = VehicleStatus.Inactive; await db.SaveChangesAsync(); return NoContent(); }
    [HttpPost("{id:int}/maintenance"), Authorize(Roles = "Admin")] public async Task<IActionResult> CompleteMaintenance(int id) { var v = await db.Vehicles.FindAsync(id); if (v is null) return NotFound(); v.Status = VehicleStatus.Available; v.LastMaintenanceDate = DateTime.UtcNow; await db.SaveChangesAsync(); return Ok(v); }
}
