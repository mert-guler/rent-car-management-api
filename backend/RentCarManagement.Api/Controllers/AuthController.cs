using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RentCarManagement.Application.Abstractions;
using RentCarManagement.Application.Contracts;

namespace RentCarManagement.Api.Controllers;

[ApiController, Route("api/auth")]
public sealed class AuthController(IAuthApplicationService auth) : ApiControllerBase
{
    [HttpPost("register"), EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(RegisterRequest request) => FromResult(await auth.RegisterAsync(request));

    [HttpPost("login"), EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(LoginRequest request) => FromResult(await auth.LoginAsync(request));

    [HttpPost("forgot-password"), EnableRateLimiting("auth")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        await auth.ForgotPasswordAsync(request.Email, $"{Request.Scheme}://{Request.Host}", HttpContext.RequestAborted);
        return Accepted(new { message = "If an account exists for this email, password reset instructions will be sent." });
    }

    [HttpPost("reset-password"), EnableRateLimiting("auth")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var result = await auth.ResetPasswordAsync(request);
        return result.Succeeded ? NoContent() : FromResult(result);
    }
}
