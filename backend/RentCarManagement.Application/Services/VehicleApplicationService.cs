using RentCarManagement.Application.Abstractions;
using RentCarManagement.Application.Contracts;
using RentCarManagement.Domain.Entities;
using RentCarManagement.Domain.Errors;

namespace RentCarManagement.Application.Services;

public sealed class VehicleApplicationService(IVehicleRepository vehicles) : IVehicleApplicationService
{
    public Task<IReadOnlyList<Vehicle>> GetAllAsync(VehicleStatus? status) => vehicles.GetAllAsync(status);

    public async Task<OperationResult<Vehicle>> GetAsync(Guid id)
    {
        var vehicle = await vehicles.GetByIdAsync(id);
        return vehicle is null ? OperationResult<Vehicle>.Failure(DomainErrorCodes.VehicleNotFound) : OperationResult<Vehicle>.Success(vehicle);
    }

    public async Task<Vehicle> CreateAsync(VehicleRequest request)
    {
        var vehicle = new Vehicle();
        Apply(vehicle, request);
        vehicles.Add(vehicle);
        await vehicles.SaveChangesAsync();
        return vehicle;
    }

    public async Task<OperationResult<bool>> UpdateAsync(Guid id, VehicleRequest request)
    {
        var vehicle = await vehicles.GetByIdAsync(id, tracking: true);
        if (vehicle is null) return OperationResult<bool>.Failure(DomainErrorCodes.VehicleNotFound);
        Apply(vehicle, request);
        await vehicles.SaveChangesAsync();
        return OperationResult<bool>.Success(true);
    }

    public async Task<OperationResult<bool>> DeactivateAsync(Guid id)
    {
        var vehicle = await vehicles.GetByIdAsync(id, tracking: true);
        if (vehicle is null) return OperationResult<bool>.Failure(DomainErrorCodes.VehicleNotFound);
        vehicle.Status = VehicleStatus.Inactive;
        await vehicles.SaveChangesAsync();
        return OperationResult<bool>.Success(true);
    }

    public async Task<OperationResult<Vehicle>> CompleteMaintenanceAsync(Guid id)
    {
        var vehicle = await vehicles.GetByIdAsync(id, tracking: true);
        if (vehicle is null) return OperationResult<Vehicle>.Failure(DomainErrorCodes.VehicleNotFound);
        if (vehicle.Status != VehicleStatus.Maintenance)
            return OperationResult<Vehicle>.Failure(DomainErrorCodes.VehicleNotAvailableForMaintenance);
        vehicle.Status = VehicleStatus.Available;
        vehicle.LastMaintenanceDate = DateTime.UtcNow;
        await vehicles.SaveChangesAsync();
        return OperationResult<Vehicle>.Success(vehicle);
    }

    private static void Apply(Vehicle vehicle, VehicleRequest request)
    {
        vehicle.Brand = request.Brand.Trim(); vehicle.Model = request.Model.Trim(); vehicle.Year = request.Year;
        vehicle.Mileage = request.Mileage; vehicle.MaintenanceMileageThreshold = request.MaintenanceMileageThreshold;
        vehicle.DailyRate = request.DailyRate; vehicle.ExtraKilometerRate = request.ExtraKilometerRate; vehicle.Status = request.Status;
    }
}
