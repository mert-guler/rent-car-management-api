using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RentCarManagement.Application.Abstractions;
using RentCarManagement.Application.Contracts;
using RentCarManagement.Domain.Entities;

namespace RentCarManagement.Infrastructure.Data.Repositories;

public sealed class AccountRepository(RentCarDbContext db) : IAccountRepository
{
    public Task<bool> EmailExistsAsync(string email) => db.Users.AnyAsync(user => user.Email == email);
    public Task<bool> NationalIdExistsAsync(string nationalId) => db.Users.AnyAsync(user => user.NationalIdNumber == nationalId);
    public Task<bool> NationalIdExistsForOtherUserAsync(Guid userId, string nationalId) => db.Users.AnyAsync(user => user.Id != userId && user.NationalIdNumber == nationalId);
    public Task<User?> FindByEmailAsync(string email) => db.Users.SingleOrDefaultAsync(user => user.Email == email);
    public Task<UserProfile?> GetProfileAsync(Guid userId) => db.Users.AsNoTracking().Where(user => user.Id == userId)
        .Select(user => new UserProfile(user.Id, user.FullName, user.Email, user.PhoneNumber, user.NationalIdNumber, user.DriverLicenseExpiry, user.Role))
        .SingleOrDefaultAsync();
    public Task<User?> GetByIdAsync(Guid userId) => db.Users.FindAsync(userId).AsTask();
    public Task<PasswordResetToken?> GetValidResetTokenAsync(string tokenHash, DateTime now) => db.PasswordResetTokens.Include(token => token.User)
        .SingleOrDefaultAsync(token => token.TokenHash == tokenHash && token.UsedAt == null && token.ExpiresAt > now);
    public async Task RevokeActiveResetTokensAsync(Guid userId, DateTime now)
    {
        var tokens = await db.PasswordResetTokens.Where(token => token.UserId == userId && token.UsedAt == null).ToListAsync();
        foreach (var token in tokens) token.UsedAt = now;
    }
    public void Add(User user) => db.Users.Add(user);
    public void Add(PasswordResetToken token) => db.PasswordResetTokens.Add(token);
    public Task SaveChangesAsync() => db.SaveChangesAsync();
}

public sealed class ContactRepository(RentCarDbContext db) : IContactRepository
{
    public void Add(SupportRequest request) => db.SupportRequests.Add(request);
    public Task SaveChangesAsync() => db.SaveChangesAsync();
}

public sealed class VehicleRepository(RentCarDbContext db) : IVehicleRepository
{
    public async Task<IReadOnlyList<Vehicle>> GetAllAsync(VehicleStatus? status) => await db.Vehicles.AsNoTracking()
        .Where(vehicle => status == null || vehicle.Status == status).ToListAsync();
    public Task<Vehicle?> GetByIdAsync(Guid id, bool tracking = false) =>
        tracking ? db.Vehicles.SingleOrDefaultAsync(vehicle => vehicle.Id == id) : db.Vehicles.AsNoTracking().SingleOrDefaultAsync(vehicle => vehicle.Id == id);
    public void Add(Vehicle vehicle) => db.Vehicles.Add(vehicle);
    public Task SaveChangesAsync() => db.SaveChangesAsync();
}

public sealed class RentalRepository(RentCarDbContext db) : IRentalRepository
{
    public async Task<IReadOnlyList<Rental>> GetHistoryAsync(Guid userId) => await db.Rentals.AsNoTracking().Include(rental => rental.Vehicle)
        .Where(rental => rental.UserId == userId).OrderByDescending(rental => rental.StartDate).ToListAsync();
    public async Task<IReadOnlyList<AdminRentalItem>> GetAdminActiveAsync() => await db.Rentals.AsNoTracking()
        .Where(rental => rental.Status == RentalStatus.Active).OrderBy(rental => rental.ExpectedReturnDate)
        .Select(rental => new AdminRentalItem(rental.Id, rental.UserId, rental.VehicleId, rental.StartDate, rental.ExpectedReturnDate,
            rental.StartMileage, rental.TotalAmount, rental.User.FullName, rental.User.Email, rental.Vehicle.Brand + " " + rental.Vehicle.Model))
        .ToListAsync();
    public Task<Vehicle?> GetVehicleAsync(Guid id) => db.Vehicles.SingleOrDefaultAsync(vehicle => vehicle.Id == id);
    public Task<Rental?> GetActiveForUserAsync(Guid rentalId, Guid userId) => db.Rentals.Include(rental => rental.Vehicle)
        .SingleOrDefaultAsync(rental => rental.Id == rentalId && rental.UserId == userId && rental.Status == RentalStatus.Active);
    public Task<Rental?> GetByUserAsync(Guid rentalId, Guid userId) => db.Rentals.Include(rental => rental.Vehicle)
        .SingleOrDefaultAsync(rental => rental.Id == rentalId && rental.UserId == userId);
    public Task<Rental?> GetActiveAsync(Guid rentalId) => db.Rentals.Include(rental => rental.Vehicle)
        .SingleOrDefaultAsync(rental => rental.Id == rentalId && rental.Status == RentalStatus.Active);
    public void Add(Rental rental) => db.Rentals.Add(rental);
    public Task SaveChangesAsync() => db.SaveChangesAsync();
}

public sealed class DamageRepository(RentCarDbContext db) : IDamageRepository
{
    public Task<bool> RentalBelongsToUserAsync(Guid rentalId, Guid userId) => db.Rentals.AnyAsync(rental => rental.Id == rentalId && rental.UserId == userId);
    public async Task<IReadOnlyList<DamageItem>> GetMineAsync(Guid userId) => await db.DamageReports.AsNoTracking()
        .Where(report => report.Rental.UserId == userId).OrderByDescending(report => report.ReportedAt)
        .Select(report => new DamageItem(report.Id, report.RentalId, report.Description, report.Amount, (int)report.Status,
            report.ReportedAt, report.AssessedAt, report.PaidAt, report.Rental.Vehicle.Brand, report.Rental.Vehicle.Model))
        .ToListAsync();
    public async Task<IReadOnlyList<DamageItem>> GetAllForAdminAsync() => await db.DamageReports.AsNoTracking()
        .OrderBy(report => report.AssessedAt).ThenByDescending(report => report.ReportedAt)
        .Select(report => new DamageItem(report.Id, report.RentalId, report.Description, report.Amount, (int)report.Status,
            report.ReportedAt, report.AssessedAt, report.PaidAt, report.Rental.Vehicle.Brand, report.Rental.Vehicle.Model,
            report.Rental.User.FullName, report.Rental.User.Email)).ToListAsync();
    public Task<DamageReport?> GetByIdAsync(Guid id, Guid? userId = null) => userId is null
        ? db.DamageReports.SingleOrDefaultAsync(report => report.Id == id)
        : db.DamageReports.SingleOrDefaultAsync(report => report.Id == id && report.Rental.UserId == userId);
    public void Add(DamageReport report) => db.DamageReports.Add(report);
    public Task SaveChangesAsync() => db.SaveChangesAsync();
}

public sealed class ApplicationSettings(IConfiguration configuration) : IApplicationSettings
{
    public string? PublicBaseUrl => configuration["App:PublicBaseUrl"];
}
