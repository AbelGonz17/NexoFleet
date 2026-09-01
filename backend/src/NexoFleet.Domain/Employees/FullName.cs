using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Employees;

public sealed record FullName
{
    public const int FirstNameMaxLength = 100;
    public const int LastNameMaxLength = 100;

    public string FirstName { get; } = string.Empty;
    public string LastName { get; } = string.Empty;

    private FullName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    private FullName() { }

    public static Result<FullName> Create(string? firstName, string? lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return Result<FullName>.Failure(EmployeeErrors.FirstNameRequired);
        }

        var trimmedFirstName = firstName.Trim();
        if (trimmedFirstName.Length > FirstNameMaxLength)
        {
            return Result<FullName>.Failure(EmployeeErrors.FirstNameTooLong);
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return Result<FullName>.Failure(EmployeeErrors.LastNameRequired);
        }

        var trimmedLastName = lastName.Trim();
        if (trimmedLastName.Length > LastNameMaxLength)
        {
            return Result<FullName>.Failure(EmployeeErrors.LastNameTooLong);
        }

        return Result<FullName>.Success(new FullName(trimmedFirstName, trimmedLastName));
    }

    public override string ToString() => $"{FirstName} {LastName}";
}
