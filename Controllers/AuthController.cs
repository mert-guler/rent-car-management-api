using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentCarManagement.Api.Data;
using RentCarManagement.Api.Models;
using RentCarManagement.Api.Services;
namespace RentCarManagement.Api.Controllers;
[ApiController, Route("api/auth")]
public sealed class AuthController(RentCarDbContext db, IPasswordService passwords, ITokenService tokens) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<TokenResponse>> Register(RegisterRequest request) { if (await db.Users.AnyAsync(x => x.Email == request.Email.ToLower())) return Conflict("Email is already registered."); var user = new User { FullName = request.FullName, Email = request.Email.ToLowerInvariant(), PasswordHash = passwords.Hash(request.Password), PhoneNumber = request.PhoneNumber, DriverLicenseNumber = request.DriverLicenseNumber, DriverLicenseExpiry = request.DriverLicenseExpiry }; db.Users.Add(user); await db.SaveChangesAsync(); return Ok(tokens.Create(user)); }
    [HttpPost("login")]
    public async Task<ActionResult<TokenResponse>> Login(LoginRequest request) { var user = await db.Users.SingleOrDefaultAsync(x => x.Email == request.Email.ToLower()); if (user is null || !passwords.Verify(request.Password, user.PasswordHash)) return Unauthorized("Invalid credentials."); return Ok(tokens.Create(user)); }
}
[ApiController, Route("api/profile"), Authorize]
public sealed class ProfileController(RentCarDbContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
    [HttpGet] public async Task<ActionResult<User>> Get() { var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == UserId); return user is null ? NotFound() : Ok(user); }
    [HttpPut] public async Task<IActionResult> Update(UpdateProfileRequest request) { var user = await db.Users.FindAsync(UserId); if (user is null) return NotFound(); user.FullName = request.FullName; user.PhoneNumber = request.PhoneNumber; user.DriverLicenseNumber = request.DriverLicenseNumber; user.DriverLicenseExpiry = request.DriverLicenseExpiry; await db.SaveChangesAsync(); return NoContent(); }
}
