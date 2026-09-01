namespace NexoFleet.Domain.Common;

public sealed record PhoneNumber
{
    public const int MaxLength = 30;

    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    public static Result<PhoneNumber> Create(
        string? phone,
        Error? requiredError = null,
        Error? tooLongError = null)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return Result<PhoneNumber>.Failure(requiredError ?? SharedErrors.PhoneRequired);
        }

        var trimmed = phone.Trim();

        if (trimmed.Length > MaxLength)
        {
            return Result<PhoneNumber>.Failure(tooLongError ?? SharedErrors.PhoneTooLong);
        }

        return Result<PhoneNumber>.Success(new PhoneNumber(trimmed));
    }

    public static implicit operator string(PhoneNumber phone) => phone.Value;

    public override string ToString() => Value;
}
