namespace RentCarManagement.Domain.Errors;

public sealed record DomainError(string Code, string Message, int HttpStatusCode = 400);

/// <summary>Central catalog of stable, searchable domain error codes.</summary>
public static class DomainErrorCodes
{
    public static readonly DomainError VehicleNotFound = new("VEHICLE_NOT_FOUND", "Vehicle was not found.", 404);
    public static readonly DomainError VehicleUnavailable = new("VEHICLE_UNAVAILABLE", "Vehicle is not available.", 409);
    public static readonly DomainError InvalidReturnDate = new("RENTAL_INVALID_RETURN_DATE", "Expected return date must be in the future.");
    public static readonly DomainError RentalNotFound = new("RENTAL_NOT_FOUND", "Rental was not found.", 404);
    public static readonly DomainError RentalAlreadyClosed = new("RENTAL_ALREADY_CLOSED", "Rental is already closed.", 409);
    public static readonly DomainError MileageBelowStart = new("RENTAL_MILEAGE_BELOW_START", "End mileage cannot be lower than start mileage.");
    public static readonly DomainError EmailAlreadyRegistered = new("AUTH_EMAIL_ALREADY_REGISTERED", "Email is already registered.", 409);
    public static readonly DomainError NationalIdAlreadyRegistered = new("PROFILE_NATIONAL_ID_ALREADY_REGISTERED", "National ID number is already registered.", 409);
    public static readonly DomainError InvalidCredentials = new("AUTH_INVALID_CREDENTIALS", "Invalid credentials.", 401);
    public static readonly DomainError InvalidResetToken = new("AUTH_RESET_TOKEN_INVALID", "Invalid or expired reset link.");
    public static readonly DomainError UserNotFound = new("USER_NOT_FOUND", "User was not found.", 404);
    public static readonly DomainError VehicleNotAvailableForMaintenance = new("VEHICLE_MAINTENANCE_INVALID_STATE", "Only a vehicle in maintenance can be marked as ready.", 409);
    public static readonly DomainError DamageAlreadyPaid = new("DAMAGE_ALREADY_PAID", "Damage report is already paid.", 409);
    public static readonly DomainError ExtendDateMustIncrease = new("RENTAL_EXTEND_DATE_MUST_INCREASE", "New date must extend the current return date.");
    public static readonly DomainError DamageAlreadyAssessed = new("DAMAGE_ALREADY_ASSESSED", "Paid damage reports cannot be reassessed.", 409);
    public static readonly DomainError DamageMustBeAssessed = new("DAMAGE_MUST_BE_ASSESSED", "An administrator must assess this damage report before simulated payment.");
    public static readonly DomainError DamageNotFound = new("DAMAGE_NOT_FOUND", "Damage report was not found.", 404);
    public static readonly DomainError Unexpected = new("SYSTEM_UNEXPECTED_ERROR", "An unexpected error occurred.", 500);
}
