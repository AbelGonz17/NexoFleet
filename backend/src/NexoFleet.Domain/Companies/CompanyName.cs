using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Companies;

public sealed record CompanyName
{
    public const int MaxLength = 200;

    public string Value { get; }

    private CompanyName(string value) => Value = value;

    public static Result<CompanyName> Create(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<CompanyName>.Failure(CompanyErrors.NameRequired);
        }

        var trimmed = name.Trim();

        if (trimmed.Length > MaxLength)
        {
            return Result<CompanyName>.Failure(CompanyErrors.NameTooLong);
        }

        return Result<CompanyName>.Success(new CompanyName(trimmed));
    }

    public static implicit operator string(CompanyName name) => name.Value;

    public override string ToString() => Value;
}
