using Microsoft.EntityFrameworkCore;
using RentCarManagement.Domain.Entities;
namespace RentCarManagement.Infrastructure.Data;
public sealed class RentCarDbContext(DbContextOptions<RentCarDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>(); public DbSet<Vehicle> Vehicles => Set<Vehicle>(); public DbSet<Rental> Rentals => Set<Rental>(); public DbSet<DamageReport> DamageReports => Set<DamageReport>();
    public DbSet<SupportRequest> SupportRequests => Set<SupportRequest>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<User>().Property(x => x.NationalIdNumber).HasMaxLength(11);
        modelBuilder.Entity<User>().HasIndex(x => x.NationalIdNumber).IsUnique().HasFilter("[NationalIdNumber] IS NOT NULL");
        modelBuilder.Entity<PasswordResetToken>().HasIndex(x => x.TokenHash).IsUnique();
        modelBuilder.Entity<PasswordResetToken>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        foreach (var entity in new[] { modelBuilder.Entity<Vehicle>().Property(x => x.DailyRate), modelBuilder.Entity<Vehicle>().Property(x => x.ExtraKilometerRate), modelBuilder.Entity<Rental>().Property(x => x.TotalAmount), modelBuilder.Entity<Rental>().Property(x => x.LateFee), modelBuilder.Entity<DamageReport>().Property(x => x.Amount) }) entity.HasPrecision(18, 2);
        modelBuilder.Entity<Rental>().HasOne(x => x.User).WithMany(x => x.Rentals).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Rental>().HasOne(x => x.Vehicle).WithMany(x => x.Rentals).HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Restrict);
    }
}
