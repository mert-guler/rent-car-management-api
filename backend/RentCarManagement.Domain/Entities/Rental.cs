namespace RentCarManagement.Domain.Entities;

public sealed class Rental
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime ExpectedReturnDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }
    public int StartMileage { get; set; }
    public int? EndMileage { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal LateFee { get; set; }
    public RentalStatus Status { get; set; } = RentalStatus.Active;
    public List<DamageReport> DamageReports { get; set; } = [];
}
