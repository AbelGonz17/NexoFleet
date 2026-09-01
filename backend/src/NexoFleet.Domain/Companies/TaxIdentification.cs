using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Companies;

public sealed record TaxIdentification
{
    public const int MaxLength = 50;

    public string Value { get; }

    private TaxIdentification(string value) => Value = value;

    public static Result<TaxIdentification> Create(string? taxId)
    {
        if (string.IsNullOrWhiteSpace(taxId))
        {
            return Result<TaxIdentification>.Failure(CompanyErrors.TaxIdentificationRequired);
        }

        var normalized = taxId.Trim().ToUpperInvariant();

        if (normalized.Length > MaxLength)
        {
            return Result<TaxIdentification>.Failure(CompanyErrors.TaxIdentificationTooLong);
        }

        return Result<TaxIdentification>.Success(new TaxIdentification(normalized));
    }

    public static implicit operator string(TaxIdentification taxId) => taxId.Value;

    public override string ToString() => Value;
}
