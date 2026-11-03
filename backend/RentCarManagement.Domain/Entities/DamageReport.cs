namespace RentCarManagement.Domain.Entities;

public sealed class DamageReport
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RentalId { get; set; }
    public Rental Rental { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DamageStatus Status { get; set; } = DamageStatus.Reported;
    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AssessedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}
