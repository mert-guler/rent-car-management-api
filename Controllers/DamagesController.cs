using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentCarManagement.Api.Data;
using RentCarManagement.Api.Models;
namespace RentCarManagement.Api.Controllers;
[ApiController, Route("api/damages"), Authorize]
public sealed class DamagesController(RentCarDbContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
    [HttpPost("rentals/{rentalId:int}")] public async Task<IActionResult> Report(int rentalId, DamageRequest request) { var rental = await db.Rentals.SingleOrDefaultAsync(x => x.Id == rentalId && x.UserId == UserId); if (rental is null) return NotFound(); var damage = new DamageReport { RentalId = rentalId, Description = request.Description, Amount = request.Amount }; db.DamageReports.Add(damage); await db.SaveChangesAsync(); return Ok(damage); }
    [HttpGet] public async Task<ActionResult<IEnumerable<DamageReport>>> GetMine() => Ok(await db.DamageReports.Include(x => x.Rental).Where(x => x.Rental.UserId == UserId).AsNoTracking().ToListAsync());
    [HttpPost("{id:int}/pay")] public async Task<IActionResult> Pay(int id) { var damage = await db.DamageReports.Include(x => x.Rental).SingleOrDefaultAsync(x => x.Id == id && x.Rental.UserId == UserId); if (damage is null) return NotFound(); if (damage.Status == DamageStatus.Paid) return BadRequest("Damage is already paid."); damage.Status = DamageStatus.Paid; damage.PaidAt = DateTime.UtcNow; await db.SaveChangesAsync(); return Ok(damage); }
}
