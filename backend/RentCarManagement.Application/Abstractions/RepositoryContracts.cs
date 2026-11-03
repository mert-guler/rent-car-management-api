using RentCarManagement.Application.Contracts;
using RentCarManagement.Domain.Entities;

namespace RentCarManagement.Application.Abstractions;

public interface IAccountRepository
{
    Task<bool> EmailExistsAsync(string email);
    Task<bool> NationalIdExistsAsync(string nationalId);
    Task<bool> NationalIdExistsForOtherUserAsync(Guid userId, string nationalId);
    Task<User?> FindByEmailAsync(string email);
    Task<UserProfile?> GetProfileAsync(Guid userId);
    Task<User?> GetByIdAsync(Guid userId);
    Task<PasswordResetToken?> GetValidResetTokenAsync(string tokenHash, DateTime now);
    Task RevokeActiveResetTokensAsync(Guid userId, DateTime now);
    void Add(User user);
    void Add(PasswordResetToken token);
    Task SaveChangesAsync();
}

public interface IContactRepository
{
    void Add(SupportRequest request);
    Task SaveChangesAsync();
}

public interface IVehicleRepository
{
    Task<IReadOnlyList<Vehicle>> GetAllAsync(VehicleStatus? status);
    Task<Vehicle?> GetByIdAsync(Guid id, bool tracking = false);
    void Add(Vehicle vehicle);
    Task SaveChangesAsync();
}

public interface IRentalRepository
{
    Task<IReadOnlyList<Rental>> GetHistoryAsync(Guid userId);
    Task<IReadOnlyList<AdminRentalItem>> GetAdminActiveAsync();
    Task<Vehicle?> GetVehicleAsync(Guid id);
    Task<Rental?> GetActiveForUserAsync(Guid rentalId, Guid userId);
    Task<Rental?> GetByUserAsync(Guid rentalId, Guid userId);
    Task<Rental?> GetActiveAsync(Guid rentalId);
    void Add(Rental rental);
    Task SaveChangesAsync();
}

public interface IDamageRepository
{
    Task<bool> RentalBelongsToUserAsync(Guid rentalId, Guid userId);
    Task<IReadOnlyList<DamageItem>> GetMineAsync(Guid userId);
    Task<IReadOnlyList<DamageItem>> GetAllForAdminAsync();
    Task<DamageReport?> GetByIdAsync(Guid id, Guid? userId = null);
    void Add(DamageReport report);
    Task SaveChangesAsync();
}

public interface IApplicationSettings
{
    string? PublicBaseUrl { get; }
}
