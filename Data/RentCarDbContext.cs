using Microsoft.EntityFrameworkCore;
using RentCarManagement.Api.Models;
namespace RentCarManagement.Api.Data;
public sealed class RentCarDbContext(DbContextOptions<RentCarDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>(); public DbSet<Vehicle> Vehicles => Set<Vehicle>(); public DbSet<Rental> Rentals => Set<Rental>(); public DbSet<DamageReport> DamageReports => Set<DamageReport>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        foreach (var entity in new[] { modelBuilder.Entity<Vehicle>().Property(x => x.DailyRate), modelBuilder.Entity<Vehicle>().Property(x => x.ExtraKilometerRate), modelBuilder.Entity<Rental>().Property(x => x.TotalAmount), modelBuilder.Entity<Rental>().Property(x => x.LateFee), modelBuilder.Entity<DamageReport>().Property(x => x.Amount) }) entity.HasPrecision(18, 2);
        modelBuilder.Entity<Rental>().HasOne(x => x.User).WithMany(x => x.Rentals).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Rental>().HasOne(x => x.Vehicle).WithMany(x => x.Rentals).HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Restrict);
    }
}
