namespace RentCarManagement.Domain.Entities;

public sealed class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer";
    public string? PhoneNumber { get; set; }
    public string? NationalIdNumber { get; set; }
    public DateTime? DriverLicenseExpiry { get; set; }
    public List<Rental> Rentals { get; set; } = [];
}
