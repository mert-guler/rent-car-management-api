using RentCarManagement.Application.Abstractions;
using RentCarManagement.Application.Contracts;
using RentCarManagement.Domain.Entities;
using RentCarManagement.Domain.Errors;

namespace RentCarManagement.Application.Services;

public sealed class RentalApplicationService(IRentalRepository rentals) : IRentalApplicationService
{
    public Task<IReadOnlyList<Rental>> GetHistoryAsync(Guid userId) => rentals.GetHistoryAsync(userId);
    public Task<IReadOnlyList<AdminRentalItem>> GetAdminActiveAsync() => rentals.GetAdminActiveAsync();

    public async Task<OperationResult<Rental>> CreateAsync(Guid userId, RentalRequest request)
    {
        var vehicle = await rentals.GetVehicleAsync(request.VehicleId);
        if (vehicle is null) return OperationResult<Rental>.Failure(DomainErrorCodes.VehicleNotFound);
        if (vehicle.Status != VehicleStatus.Available) return OperationResult<Rental>.Failure(DomainErrorCodes.VehicleUnavailable);
        if (request.ExpectedReturnDate <= DateTime.UtcNow) return OperationResult<Rental>.Failure(DomainErrorCodes.InvalidReturnDate);

        var now = DateTime.UtcNow;
        var days = Math.Max(1, (request.ExpectedReturnDate.Date - now.Date).Days);
        var rental = new Rental
        {
            UserId = userId, VehicleId = vehicle.Id, StartDate = now, ExpectedReturnDate = request.ExpectedReturnDate,
            StartMileage = vehicle.Mileage, TotalAmount = days * vehicle.DailyRate
        };
        vehicle.Status = VehicleStatus.Rented;
        rentals.Add(rental);
        await rentals.SaveChangesAsync();
        return OperationResult<Rental>.Success(rental);
    }

    public async Task<OperationResult<Rental>> ExtendAsync(Guid userId, Guid rentalId, ExtendRentalRequest request)
    {
        var rental = await rentals.GetActiveForUserAsync(rentalId, userId);
        if (rental is null) return OperationResult<Rental>.Failure(DomainErrorCodes.RentalNotFound);
        if (request.NewExpectedReturnDate <= rental.ExpectedReturnDate)
            return OperationResult<Rental>.Failure(DomainErrorCodes.ExtendDateMustIncrease);
        var days = (request.NewExpectedReturnDate.Date - rental.ExpectedReturnDate.Date).Days;
        rental.ExpectedReturnDate = request.NewExpectedReturnDate;
        rental.TotalAmount += days * rental.Vehicle.DailyRate;
        await rentals.SaveChangesAsync();
        return OperationResult<Rental>.Success(rental);
    }

    public Task<OperationResult<Rental>> ReturnAsync(Guid userId, Guid rentalId, ReturnRentalRequest request) =>
        ReturnCoreAsync(awaitActive: false, userId, rentalId, request);

    public Task<OperationResult<Rental>> AdminReturnAsync(Guid rentalId, ReturnRentalRequest request) =>
        ReturnCoreAsync(awaitActive: true, Guid.Empty, rentalId, request);

    private async Task<OperationResult<Rental>> ReturnCoreAsync(bool awaitActive, Guid userId, Guid rentalId, ReturnRentalRequest request)
    {
        var rental = awaitActive ? await rentals.GetActiveAsync(rentalId) : await rentals.GetByUserAsync(rentalId, userId);
        if (rental is null) return OperationResult<Rental>.Failure(DomainErrorCodes.RentalNotFound);
        if (rental.Status != RentalStatus.Active) return OperationResult<Rental>.Failure(DomainErrorCodes.RentalAlreadyClosed);
        if (request.EndMileage < rental.StartMileage) return OperationResult<Rental>.Failure(DomainErrorCodes.MileageBelowStart);

        var now = DateTime.UtcNow;
        var lateDays = Math.Max(0, (now.Date - rental.ExpectedReturnDate.Date).Days);
        rental.LateFee = lateDays * rental.Vehicle.DailyRate;
        var allowanceDays = Math.Max(1, (rental.ExpectedReturnDate.Date - rental.StartDate.Date).Days);
        var excessKm = Math.Max(0, request.EndMileage - rental.StartMileage - 200 * allowanceDays);
        rental.TotalAmount += rental.LateFee + excessKm * rental.Vehicle.ExtraKilometerRate;
        rental.ActualReturnDate = now; rental.EndMileage = request.EndMileage; rental.Status = RentalStatus.Returned;
        rental.Vehicle.Mileage = request.EndMileage;
        rental.Vehicle.Status = rental.Vehicle.Mileage >= rental.Vehicle.MaintenanceMileageThreshold
            ? VehicleStatus.Maintenance : VehicleStatus.Available;
        await rentals.SaveChangesAsync();
        return OperationResult<Rental>.Success(rental);
    }
}
