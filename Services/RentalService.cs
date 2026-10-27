using Microsoft.EntityFrameworkCore;
using RentCarManagement.Api.Data;
using RentCarManagement.Api.Models;
namespace RentCarManagement.Api.Services;
public interface IRentalService { Task<(Rental? Rental, string? Error)> CreateAsync(int userId, RentalRequest request); Task<(Rental? Rental, string? Error)> ReturnAsync(int userId, int rentalId, ReturnRentalRequest request); }
public sealed class RentalService(RentCarDbContext db, ILogger<RentalService> logger) : IRentalService
{
    public async Task<(Rental? Rental, string? Error)> CreateAsync(int userId, RentalRequest request)
    {
        var vehicle = await db.Vehicles.FindAsync(request.VehicleId); if (vehicle is null) return (null, "Vehicle not found."); if (vehicle.Status != VehicleStatus.Available) return (null, "Vehicle is not available."); if (request.ExpectedReturnDate <= DateTime.UtcNow) return (null, "Expected return date must be in the future.");
        var days = Math.Max(1, (request.ExpectedReturnDate.Date - DateTime.UtcNow.Date).Days); var rental = new Rental { UserId = userId, VehicleId = vehicle.Id, StartDate = DateTime.UtcNow, ExpectedReturnDate = request.ExpectedReturnDate, StartMileage = vehicle.Mileage, TotalAmount = days * vehicle.DailyRate };
        vehicle.Status = VehicleStatus.Rented; db.Rentals.Add(rental); await db.SaveChangesAsync(); return (rental, null);
    }
    public async Task<(Rental? Rental, string? Error)> ReturnAsync(int userId, int rentalId, ReturnRentalRequest request)
    {
        var rental = await db.Rentals.Include(x => x.Vehicle).FirstOrDefaultAsync(x => x.Id == rentalId && x.UserId == userId); if (rental is null) return (null, "Rental not found."); if (rental.Status != RentalStatus.Active) return (null, "Rental is already closed."); if (request.EndMileage < rental.StartMileage) return (null, "End mileage cannot be lower than start mileage.");
        var now = DateTime.UtcNow; var lateDays = Math.Max(0, (now.Date - rental.ExpectedReturnDate.Date).Days); rental.LateFee = lateDays * rental.Vehicle.DailyRate; var extraKm = Math.Max(0, request.EndMileage - rental.StartMileage - 200 * Math.Max(1, (rental.ExpectedReturnDate.Date - rental.StartDate.Date).Days)); rental.TotalAmount += rental.LateFee + extraKm * rental.Vehicle.ExtraKilometerRate; rental.ActualReturnDate = now; rental.EndMileage = request.EndMileage; rental.Status = RentalStatus.Returned; rental.Vehicle.Mileage = request.EndMileage; rental.Vehicle.Status = rental.Vehicle.Mileage >= rental.Vehicle.MaintenanceMileageThreshold ? VehicleStatus.Maintenance : VehicleStatus.Available; logger.LogInformation("Rental {RentalId} returned with total {TotalAmount}", rentalId, rental.TotalAmount); await db.SaveChangesAsync(); return (rental, null);
    }
}
