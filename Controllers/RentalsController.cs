using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentCarManagement.Api.Data;
using RentCarManagement.Api.Models;
using RentCarManagement.Api.Services;
namespace RentCarManagement.Api.Controllers;
[ApiController, Route("api/rentals"), Authorize]
public sealed class RentalsController(RentCarDbContext db, IRentalService rentals) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
    [HttpGet] public async Task<ActionResult<IEnumerable<Rental>>> History() => Ok(await db.Rentals.AsNoTracking().Include(x => x.Vehicle).Where(x => x.UserId == UserId).OrderByDescending(x => x.StartDate).ToListAsync());
    [HttpGet("admin"), Authorize(Roles = "Admin")] public async Task<IActionResult> AdminActiveRentals() => Ok(await db.Rentals.AsNoTracking().Where(x => x.Status == RentalStatus.Active).OrderBy(x => x.ExpectedReturnDate).Select(x => new { x.Id, x.UserId, x.VehicleId, x.StartDate, x.ExpectedReturnDate, x.StartMileage, x.TotalAmount, Customer = x.User.FullName, x.User.Email, Vehicle = x.Vehicle.Brand + " " + x.Vehicle.Model }).ToListAsync());
    [HttpPost("admin/{id:int}/return"), Authorize(Roles = "Admin")] public async Task<IActionResult> AdminReturn(int id, ReturnRentalRequest request) { var rental = await db.Rentals.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.Status == RentalStatus.Active); if (rental is null) return NotFound("Active rental not found."); var result = await rentals.ReturnAsync(rental.UserId, id, request); return result.Rental is null ? BadRequest(result.Error) : Ok(result.Rental); }
    [HttpPost] public async Task<IActionResult> Create(RentalRequest request) { var result = await rentals.CreateAsync(UserId, request); return result.Rental is null ? BadRequest(result.Error) : Ok(result.Rental); }
    [HttpPut("{id:int}/extend")] public async Task<IActionResult> Extend(int id, ExtendRentalRequest request) { var rental = await db.Rentals.Include(x => x.Vehicle).SingleOrDefaultAsync(x => x.Id == id && x.UserId == UserId && x.Status == RentalStatus.Active); if (rental is null) return NotFound(); if (request.NewExpectedReturnDate <= rental.ExpectedReturnDate) return BadRequest("New date must extend the current return date."); var extraDays = (request.NewExpectedReturnDate.Date - rental.ExpectedReturnDate.Date).Days; rental.ExpectedReturnDate = request.NewExpectedReturnDate; rental.TotalAmount += extraDays * rental.Vehicle.DailyRate; await db.SaveChangesAsync(); return Ok(rental); }
    [HttpPost("{id:int}/return")] public async Task<IActionResult> Return(int id, ReturnRentalRequest request) { var result = await rentals.ReturnAsync(UserId, id, request); return result.Rental is null ? BadRequest(result.Error) : Ok(result.Rental); }
}
