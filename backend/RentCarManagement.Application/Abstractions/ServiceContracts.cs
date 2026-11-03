using RentCarManagement.Application.Contracts;
using RentCarManagement.Domain.Entities;

namespace RentCarManagement.Application.Abstractions;

public interface IPasswordService { string Hash(string password); bool Verify(string password, string hash); }
public interface ITokenService { TokenResponse Create(User user); }
public interface IPasswordResetNotifier { Task SendAsync(string email, string resetUrl, CancellationToken cancellationToken); }

public interface IAuthApplicationService
{
    Task<OperationResult<TokenResponse>> RegisterAsync(RegisterRequest request);
    Task<OperationResult<TokenResponse>> LoginAsync(LoginRequest request);
    Task ForgotPasswordAsync(string email, string resetBaseUrl, CancellationToken cancellationToken);
    Task<OperationResult<bool>> ResetPasswordAsync(ResetPasswordRequest request);
}

public interface IProfileApplicationService
{
    Task<OperationResult<UserProfile>> GetAsync(Guid userId);
    Task<OperationResult<bool>> UpdateAsync(Guid userId, UpdateProfileRequest request);
}

public interface IContactApplicationService
{
    Task<ContactReceipt> CreateAsync(ContactRequest request);
}

public interface IVehicleApplicationService
{
    Task<IReadOnlyList<Vehicle>> GetAllAsync(VehicleStatus? status);
    Task<OperationResult<Vehicle>> GetAsync(Guid id);
    Task<Vehicle> CreateAsync(VehicleRequest request);
    Task<OperationResult<bool>> UpdateAsync(Guid id, VehicleRequest request);
    Task<OperationResult<bool>> DeactivateAsync(Guid id);
    Task<OperationResult<Vehicle>> CompleteMaintenanceAsync(Guid id);
}

public interface IRentalApplicationService
{
    Task<IReadOnlyList<Rental>> GetHistoryAsync(Guid userId);
    Task<IReadOnlyList<AdminRentalItem>> GetAdminActiveAsync();
    Task<OperationResult<Rental>> CreateAsync(Guid userId, RentalRequest request);
    Task<OperationResult<Rental>> ExtendAsync(Guid userId, Guid rentalId, ExtendRentalRequest request);
    Task<OperationResult<Rental>> ReturnAsync(Guid userId, Guid rentalId, ReturnRentalRequest request);
    Task<OperationResult<Rental>> AdminReturnAsync(Guid rentalId, ReturnRentalRequest request);
}

public interface IDamageApplicationService
{
    Task<OperationResult<DamageReceipt>> ReportAsync(Guid userId, Guid rentalId, DamageRequest request);
    Task<IReadOnlyList<DamageItem>> GetMineAsync(Guid userId);
    Task<IReadOnlyList<DamageItem>> GetAllForAdminAsync();
    Task<OperationResult<bool>> AssessAsync(Guid id, AssessDamageRequest request);
    Task<OperationResult<DamagePaymentReceipt>> PayAsync(Guid userId, Guid id);
}
