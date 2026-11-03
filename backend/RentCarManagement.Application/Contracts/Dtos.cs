using System.ComponentModel.DataAnnotations;
using RentCarManagement.Domain.Entities;

namespace RentCarManagement.Application.Contracts;

public sealed record RegisterRequest([Required, StringLength(120, MinimumLength = 2)] string FullName, [Required, EmailAddress, StringLength(254)] string Email, [Required, MinLength(8), StringLength(128)] string Password, string? PhoneNumber, [RegularExpression("^[1-9][0-9]{10}$")] string? NationalIdNumber, DateTime? DriverLicenseExpiry);
public sealed record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);
public sealed record UpdateProfileRequest([Required] string FullName, string? PhoneNumber, [Required, RegularExpression("^[1-9][0-9]{10}$")] string NationalIdNumber, DateTime? DriverLicenseExpiry);
public sealed record VehicleRequest([Required] string Brand, [Required] string Model, [Range(1900, 2100)] int Year, [Range(0, int.MaxValue)] int Mileage, [Range(0, int.MaxValue)] int MaintenanceMileageThreshold, [Range(0, double.MaxValue)] decimal DailyRate, [Range(0, double.MaxValue)] decimal ExtraKilometerRate, VehicleStatus Status = VehicleStatus.Available);
public sealed record RentalRequest(Guid VehicleId, DateTime ExpectedReturnDate);
public sealed record ExtendRentalRequest(DateTime NewExpectedReturnDate);
public sealed record ReturnRentalRequest([Range(0, int.MaxValue)] int EndMileage);
public sealed record DamageRequest([Required, StringLength(2000, MinimumLength = 5)] string Description);
public sealed record AssessDamageRequest([Range(0.01, double.MaxValue)] decimal Amount);
public sealed record TokenResponse(string AccessToken, DateTime ExpiresAt, string Role);
public sealed record ContactRequest([Required, StringLength(120)] string Name, [Required, EmailAddress] string Email, [Required, StringLength(160)] string Subject, [Required, StringLength(3000, MinimumLength = 10)] string Message);
public sealed record ForgotPasswordRequest([Required, EmailAddress] string Email);
public sealed record ResetPasswordRequest([Required] string Token, [Required, MinLength(8)] string NewPassword);
public sealed record UserProfile(Guid Id, string FullName, string Email, string? PhoneNumber, string? NationalIdNumber, DateTime? DriverLicenseExpiry, string Role);
public sealed record AdminRentalItem(Guid Id, Guid UserId, Guid VehicleId, DateTime StartDate, DateTime ExpectedReturnDate, int StartMileage, decimal TotalAmount, string Customer, string Email, string Vehicle);
public sealed record DamageItem(Guid Id, Guid RentalId, string Description, decimal Amount, int Status, DateTime ReportedAt, DateTime? AssessedAt, DateTime? PaidAt, string Brand, string Model, string? FullName = null, string? Email = null);
public sealed record ContactReceipt(Guid Id, DateTime CreatedAt, string Message);
public sealed record DamageReceipt(Guid Id, Guid RentalId, string Description, DateTime ReportedAt, int Status);
public sealed record DamagePaymentReceipt(Guid Id, decimal Amount, int Status, DateTime? PaidAt, bool Simulated);
