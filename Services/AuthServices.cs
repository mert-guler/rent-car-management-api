using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Net.Mail;
using Microsoft.IdentityModel.Tokens;
using RentCarManagement.Api.Models;

namespace RentCarManagement.Api.Services;
public interface IPasswordService { string Hash(string password); bool Verify(string password, string hash); }
public sealed class PasswordService : IPasswordService
{
    public string Hash(string password) { var salt = RandomNumberGenerator.GetBytes(16); var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32); return $"100000.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}"; }
    public bool Verify(string password, string hash) { var parts = hash.Split('.'); if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations)) return false; var key = Rfc2898DeriveBytes.Pbkdf2(password, Convert.FromBase64String(parts[1]), iterations, HashAlgorithmName.SHA256, 32); return CryptographicOperations.FixedTimeEquals(key, Convert.FromBase64String(parts[2])); }
}
public interface ITokenService { TokenResponse Create(User user); }
public sealed class TokenService(IConfiguration configuration) : ITokenService
{
    public TokenResponse Create(User user)
    {
        var expires = DateTime.UtcNow.AddHours(2); var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Email, user.Email), new Claim(ClaimTypes.Role, user.Role), new Claim(ClaimTypes.Name, user.FullName) };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)); var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); var token = new JwtSecurityToken(configuration["Jwt:Issuer"], configuration["Jwt:Audience"], claims, expires: expires, signingCredentials: credentials);
        return new TokenResponse(new JwtSecurityTokenHandler().WriteToken(token), expires, user.Role);
    }
}

public interface IPasswordResetNotifier
{
    Task SendAsync(string email, string resetUrl, CancellationToken cancellationToken);
}

public sealed class PasswordResetNotifier(IConfiguration configuration, IWebHostEnvironment environment, ILogger<PasswordResetNotifier> logger) : IPasswordResetNotifier
{
    public async Task SendAsync(string email, string resetUrl, CancellationToken cancellationToken)
    {
        var host = configuration["Smtp:Host"];
        if (string.IsNullOrWhiteSpace(host) && environment.IsDevelopment())
        {
            logger.LogInformation("Development password reset link: {ResetUrl}", resetUrl);
            return;
        }

        if (string.IsNullOrWhiteSpace(host))
            throw new InvalidOperationException("SMTP must be configured to send password reset emails outside Development.");

#pragma warning disable SYSLIB0014
        using var client = new SmtpClient(host, configuration.GetValue<int?>("Smtp:Port") ?? 587)
        {
            EnableSsl = configuration.GetValue("Smtp:EnableSsl", true),
            Credentials = string.IsNullOrWhiteSpace(configuration["Smtp:Username"])
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(configuration["Smtp:Username"], configuration["Smtp:Password"])
        };
        using var message = new MailMessage(configuration["Smtp:From"] ?? "noreply@localhost", email)
        {
            Subject = "Reset your yol. password",
            Body = $"Use this link within 30 minutes to reset your password: {resetUrl}",
            IsBodyHtml = false
        };
        await client.SendMailAsync(message, cancellationToken);
#pragma warning restore SYSLIB0014
    }
}
