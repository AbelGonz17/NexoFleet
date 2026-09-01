using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Employees;

public sealed record EmployeeCode
{
    public const int MaxLength = 50;

    public string Value { get; }

    private EmployeeCode(string value) => Value = value;

    public static Result<EmployeeCode> Create(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result<EmployeeCode>.Failure(EmployeeErrors.EmployeeCodeRequired);
        }

        var normalized = code.Trim().ToUpperInvariant();

        if (normalized.Length > MaxLength)
        {
            return Result<EmployeeCode>.Failure(EmployeeErrors.EmployeeCodeTooLong);
        }

        return Result<EmployeeCode>.Success(new EmployeeCode(normalized));
    }

    public static implicit operator string(EmployeeCode code) => code.Value;

    public override string ToString() => Value;
}
