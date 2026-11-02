using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentCarManagement.Api.Data;
using RentCarManagement.Api.Models;

namespace RentCarManagement.Api.Controllers;

[ApiController, Route("api/damages"), Authorize]
public sealed class DamagesController(RentCarDbContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpPost("rentals/{rentalId:int}")]
    public async Task<IActionResult> Report(int rentalId, DamageRequest request)
    {
        var rental = await db.Rentals.SingleOrDefaultAsync(x => x.Id == rentalId && x.UserId == UserId);
        if (rental is null) return NotFound();

        var damage = new DamageReport { RentalId = rentalId, Description = request.Description.Trim() };
        db.DamageReports.Add(damage);
        await db.SaveChangesAsync();
        return Ok(new { damage.Id, damage.RentalId, damage.Description, damage.ReportedAt, damage.Status });
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetMine()
    {
        var items = await db.DamageReports.AsNoTracking()
            .Where(x => x.Rental.UserId == UserId)
            .OrderByDescending(x => x.ReportedAt)
            .Select(x => new { x.Id, x.RentalId, x.Description, x.Amount, x.Status, x.ReportedAt, x.AssessedAt, x.PaidAt, x.Rental.Vehicle.Brand, x.Rental.Vehicle.Model })
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("admin"), Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<object>>> GetAllForAdmin()
    {
        var items = await db.DamageReports.AsNoTracking()
            .OrderBy(x => x.AssessedAt).ThenByDescending(x => x.ReportedAt)
            .Select(x => new { x.Id, x.RentalId, x.Description, x.Amount, x.Status, x.ReportedAt, x.AssessedAt, x.PaidAt, x.Rental.User.FullName, x.Rental.User.Email, x.Rental.Vehicle.Brand, x.Rental.Vehicle.Model })
            .ToListAsync();
        return Ok(items);
    }

    [HttpPut("{id:int}/assess"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Assess(int id, AssessDamageRequest request)
    {
        var damage = await db.DamageReports.FindAsync(id);
        if (damage is null) return NotFound();
        if (damage.Status == DamageStatus.Paid) return BadRequest("Paid damage reports cannot be reassessed.");

        damage.Amount = request.Amount;
        damage.AssessedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/pay")]
    public async Task<IActionResult> Pay(int id)
    {
        var damage = await db.DamageReports.SingleOrDefaultAsync(x => x.Id == id && x.Rental.UserId == UserId);
        if (damage is null) return NotFound();
        if (damage.AssessedAt is null || damage.Amount <= 0) return BadRequest("An administrator must assess this damage report before simulated payment.");
        if (damage.Status == DamageStatus.Paid) return BadRequest("Damage is already marked as paid.");

        damage.Status = DamageStatus.Paid;
        damage.PaidAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(new { damage.Id, damage.Amount, damage.Status, damage.PaidAt, Simulated = true });
    }
}
