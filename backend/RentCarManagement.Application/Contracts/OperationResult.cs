using RentCarManagement.Domain.Errors;

namespace RentCarManagement.Application.Contracts;

public sealed record OperationResult<T>(T? Value, DomainError? Error)
{
    public bool Succeeded => Error is null;
    public static OperationResult<T> Success(T value) => new(value, null);
    public static OperationResult<T> Failure(DomainError error) => new(default, error);
}
