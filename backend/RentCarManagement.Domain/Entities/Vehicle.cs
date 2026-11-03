namespace RentCarManagement.Domain.Entities;

public sealed class Vehicle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Mileage { get; set; }
    public int MaintenanceMileageThreshold { get; set; } = 10_000;
    public VehicleStatus Status { get; set; } = VehicleStatus.Available;
    public decimal DailyRate { get; set; }
    public decimal ExtraKilometerRate { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public List<Rental> Rentals { get; set; } = [];
}
