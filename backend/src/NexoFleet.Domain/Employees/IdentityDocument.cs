using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Employees;

public sealed record IdentityDocument
{
    public const int MaxLength = 50;

    public string Value { get; }

    private IdentityDocument(string value) => Value = value;

    public static Result<IdentityDocument> Create(string? document)
    {
        if (string.IsNullOrWhiteSpace(document))
        {
            return Result<IdentityDocument>.Failure(EmployeeErrors.IdentityDocumentRequired);
        }

        var normalized = document.Trim().ToUpperInvariant();

        if (normalized.Length > MaxLength)
        {
            return Result<IdentityDocument>.Failure(EmployeeErrors.IdentityDocumentTooLong);
        }

        return Result<IdentityDocument>.Success(new IdentityDocument(normalized));
    }

    public static implicit operator string(IdentityDocument document) => document.Value;

    public override string ToString() => Value;
}
