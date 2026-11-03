using System.Security.Cryptography;
using System.Text;
using RentCarManagement.Application.Abstractions;
using RentCarManagement.Application.Contracts;
using RentCarManagement.Domain.Entities;
using RentCarManagement.Domain.Errors;

namespace RentCarManagement.Application.Services;

public sealed class AuthApplicationService(
    IAccountRepository accounts,
    IPasswordService passwords,
    ITokenService tokens,
    IPasswordResetNotifier resetNotifier,
    IApplicationSettings settings) : IAuthApplicationService
{
    public async Task<OperationResult<TokenResponse>> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await accounts.EmailExistsAsync(email))
            return OperationResult<TokenResponse>.Failure(DomainErrorCodes.EmailAlreadyRegistered);
        if (request.NationalIdNumber is not null && await accounts.NationalIdExistsAsync(request.NationalIdNumber))
            return OperationResult<TokenResponse>.Failure(DomainErrorCodes.NationalIdAlreadyRegistered);

        var user = new User
        {
            FullName = request.FullName.Trim(), Email = email, PasswordHash = passwords.Hash(request.Password),
            PhoneNumber = request.PhoneNumber?.Trim(), NationalIdNumber = request.NationalIdNumber,
            DriverLicenseExpiry = request.DriverLicenseExpiry
        };
        accounts.Add(user);
        await accounts.SaveChangesAsync();
        return OperationResult<TokenResponse>.Success(tokens.Create(user));
    }

    public async Task<OperationResult<TokenResponse>> LoginAsync(LoginRequest request)
    {
        var user = await accounts.FindByEmailAsync(request.Email.Trim().ToLowerInvariant());
        return user is null || !passwords.Verify(request.Password, user.PasswordHash)
            ? OperationResult<TokenResponse>.Failure(DomainErrorCodes.InvalidCredentials)
            : OperationResult<TokenResponse>.Success(tokens.Create(user));
    }

    public async Task ForgotPasswordAsync(string email, string resetBaseUrl, CancellationToken cancellationToken)
    {
        var user = await accounts.FindByEmailAsync(email.Trim().ToLowerInvariant());
        if (user is null) return;

        var now = DateTime.UtcNow;
        await accounts.RevokeActiveResetTokensAsync(user.Id, now);
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        accounts.Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken))),
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(30)
        });
        await accounts.SaveChangesAsync();

        var baseUrl = settings.PublicBaseUrl?.TrimEnd('/') ?? resetBaseUrl.TrimEnd('/');
        var resetUrl = $"{baseUrl}/?resetToken={Uri.EscapeDataString(rawToken)}";
        await resetNotifier.SendAsync(user.Email, resetUrl, cancellationToken);
    }

    public async Task<OperationResult<bool>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Token)));
        var now = DateTime.UtcNow;
        var resetToken = await accounts.GetValidResetTokenAsync(hash, now);
        if (resetToken is null) return OperationResult<bool>.Failure(DomainErrorCodes.InvalidResetToken);

        resetToken.User.PasswordHash = passwords.Hash(request.NewPassword);
        await accounts.RevokeActiveResetTokensAsync(resetToken.UserId, now);
        await accounts.SaveChangesAsync();
        return OperationResult<bool>.Success(true);
    }
}

public sealed class ProfileApplicationService(IAccountRepository accounts) : IProfileApplicationService
{
    public async Task<OperationResult<UserProfile>> GetAsync(Guid userId)
    {
        var profile = await accounts.GetProfileAsync(userId);
        return profile is null
            ? OperationResult<UserProfile>.Failure(DomainErrorCodes.UserNotFound)
            : OperationResult<UserProfile>.Success(profile);
    }

    public async Task<OperationResult<bool>> UpdateAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await accounts.GetByIdAsync(userId);
        if (user is null) return OperationResult<bool>.Failure(DomainErrorCodes.UserNotFound);
        if (await accounts.NationalIdExistsForOtherUserAsync(userId, request.NationalIdNumber))
            return OperationResult<bool>.Failure(DomainErrorCodes.NationalIdAlreadyRegistered);
        user.FullName = request.FullName.Trim();
        user.PhoneNumber = request.PhoneNumber?.Trim();
        user.NationalIdNumber = request.NationalIdNumber;
        user.DriverLicenseExpiry = request.DriverLicenseExpiry;
        await accounts.SaveChangesAsync();
        return OperationResult<bool>.Success(true);
    }
}
