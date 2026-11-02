using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentCarManagement.Api.Data;
using RentCarManagement.Api.Models;
using RentCarManagement.Api.Services;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.RateLimiting;
namespace RentCarManagement.Api.Controllers;
[ApiController, Route("api/auth")]
public sealed class AuthController(RentCarDbContext db, IPasswordService passwords, ITokenService tokens, IPasswordResetNotifier resetNotifier, IConfiguration configuration, ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("register"), EnableRateLimiting("auth")]
    public async Task<ActionResult<TokenResponse>> Register(RegisterRequest request) { if (await db.Users.AnyAsync(x => x.Email == request.Email.ToLower())) return Conflict("Email is already registered."); if (request.NationalIdNumber is not null && await db.Users.AnyAsync(x => x.NationalIdNumber == request.NationalIdNumber)) return Conflict("National ID number is already registered."); var user = new User { FullName = request.FullName, Email = request.Email.ToLowerInvariant(), PasswordHash = passwords.Hash(request.Password), PhoneNumber = request.PhoneNumber, NationalIdNumber = request.NationalIdNumber, DriverLicenseExpiry = request.DriverLicenseExpiry }; db.Users.Add(user); await db.SaveChangesAsync(); return Ok(tokens.Create(user)); }
    [HttpPost("login"), EnableRateLimiting("auth")]
    public async Task<ActionResult<TokenResponse>> Login(LoginRequest request) { var user = await db.Users.SingleOrDefaultAsync(x => x.Email == request.Email.ToLower()); if (user is null || !passwords.Verify(request.Password, user.PasswordHash)) return Unauthorized("Invalid credentials."); return Ok(tokens.Create(user)); }

    [HttpPost("forgot-password"), EnableRateLimiting("auth")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        const string response = "If an account exists for this email, password reset instructions will be sent.";
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email);
        if (user is null) return Accepted(new { message = response });

        var now = DateTime.UtcNow;
        var previousTokens = await db.PasswordResetTokens.Where(x => x.UserId == user.Id && x.UsedAt == null).ToListAsync();
        foreach (var previous in previousTokens) previous.UsedAt = now;

        var rawToken = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
        db.PasswordResetTokens.Add(new PasswordResetToken { UserId = user.Id, TokenHash = tokenHash, CreatedAt = now, ExpiresAt = now.AddMinutes(30) });
        await db.SaveChangesAsync();

        var baseUrl = configuration["App:PublicBaseUrl"]?.TrimEnd('/') ?? $"{Request.Scheme}://{Request.Host}";
        var resetUrl = $"{baseUrl}/?resetToken={Uri.EscapeDataString(rawToken)}";
        try { await resetNotifier.SendAsync(user.Email, resetUrl, HttpContext.RequestAborted); }
        catch (Exception exception) { logger.LogError(exception, "Password reset email delivery failed."); }

        return Accepted(new { message = response });
    }

    [HttpPost("reset-password"), EnableRateLimiting("auth")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        string tokenHash;
        try { tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Token))); }
        catch (ArgumentException) { return BadRequest("Invalid or expired reset link."); }

        var now = DateTime.UtcNow;
        var resetToken = await db.PasswordResetTokens.Include(x => x.User)
            .SingleOrDefaultAsync(x => x.TokenHash == tokenHash && x.UsedAt == null && x.ExpiresAt > now);
        if (resetToken is null) return BadRequest("Invalid or expired reset link.");

        resetToken.User.PasswordHash = passwords.Hash(request.NewPassword);
        var activeTokens = await db.PasswordResetTokens.Where(x => x.UserId == resetToken.UserId && x.UsedAt == null).ToListAsync();
        foreach (var item in activeTokens) item.UsedAt = now;
        await db.SaveChangesAsync();
        return NoContent();
    }
}
[ApiController, Route("api/profile"), Authorize]
public sealed class ProfileController(RentCarDbContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
    [HttpGet] public async Task<IActionResult> Get() { var user = await db.Users.AsNoTracking().Where(x => x.Id == UserId).Select(x => new { x.Id, x.FullName, x.Email, x.PhoneNumber, x.NationalIdNumber, x.DriverLicenseExpiry, x.Role }).SingleOrDefaultAsync(); return user is null ? NotFound() : Ok(user); }
    [HttpPut] public async Task<IActionResult> Update(UpdateProfileRequest request) { var user = await db.Users.FindAsync(UserId); if (user is null) return NotFound(); if (await db.Users.AnyAsync(x => x.Id != UserId && x.NationalIdNumber == request.NationalIdNumber)) return Conflict("National ID number is already registered."); user.FullName = request.FullName; user.PhoneNumber = request.PhoneNumber; user.NationalIdNumber = request.NationalIdNumber; user.DriverLicenseExpiry = request.DriverLicenseExpiry; await db.SaveChangesAsync(); return NoContent(); }
}
