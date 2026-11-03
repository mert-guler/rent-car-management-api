using RentCarManagement.Application.Abstractions;
using RentCarManagement.Application.Contracts;
using RentCarManagement.Domain.Entities;
using RentCarManagement.Domain.Errors;

namespace RentCarManagement.Application.Services;

public sealed class DamageApplicationService(IDamageRepository damages) : IDamageApplicationService
{
    public async Task<OperationResult<DamageReceipt>> ReportAsync(Guid userId, Guid rentalId, DamageRequest request)
    {
        if (!await damages.RentalBelongsToUserAsync(rentalId, userId))
            return OperationResult<DamageReceipt>.Failure(DomainErrorCodes.RentalNotFound);
        var report = new DamageReport { RentalId = rentalId, Description = request.Description.Trim() };
        damages.Add(report);
        await damages.SaveChangesAsync();
        return OperationResult<DamageReceipt>.Success(new DamageReceipt(report.Id, report.RentalId, report.Description,
            report.ReportedAt, (int)report.Status));
    }

    public Task<IReadOnlyList<DamageItem>> GetMineAsync(Guid userId) => damages.GetMineAsync(userId);
    public Task<IReadOnlyList<DamageItem>> GetAllForAdminAsync() => damages.GetAllForAdminAsync();

    public async Task<OperationResult<bool>> AssessAsync(Guid id, AssessDamageRequest request)
    {
        var report = await damages.GetByIdAsync(id);
        if (report is null) return OperationResult<bool>.Failure(DomainErrorCodes.DamageNotFound);
        if (report.Status == DamageStatus.Paid) return OperationResult<bool>.Failure(DomainErrorCodes.DamageAlreadyAssessed);
        report.Amount = request.Amount; report.AssessedAt = DateTime.UtcNow;
        await damages.SaveChangesAsync();
        return OperationResult<bool>.Success(true);
    }

    public async Task<OperationResult<DamagePaymentReceipt>> PayAsync(Guid userId, Guid id)
    {
        var report = await damages.GetByIdAsync(id, userId);
        if (report is null) return OperationResult<DamagePaymentReceipt>.Failure(DomainErrorCodes.DamageNotFound);
        if (report.AssessedAt is null || report.Amount <= 0) return OperationResult<DamagePaymentReceipt>.Failure(DomainErrorCodes.DamageMustBeAssessed);
        if (report.Status == DamageStatus.Paid) return OperationResult<DamagePaymentReceipt>.Failure(DomainErrorCodes.DamageAlreadyPaid);
        report.Status = DamageStatus.Paid; report.PaidAt = DateTime.UtcNow;
        await damages.SaveChangesAsync();
        return OperationResult<DamagePaymentReceipt>.Success(new DamagePaymentReceipt(report.Id, report.Amount,
            (int)report.Status, report.PaidAt, Simulated: true));
    }
}
