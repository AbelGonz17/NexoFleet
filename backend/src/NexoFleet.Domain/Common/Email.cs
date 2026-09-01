using System.Text.RegularExpressions;

namespace NexoFleet.Domain.Common;

public sealed partial record Email
{
    public const int MaxLength = 256;

    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Result<Email> Create(
        string? email,
        Error? invalidError = null,
        Error? tooLongError = null,
        Error? requiredError = null)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result<Email>.Failure(requiredError ?? invalidError ?? SharedErrors.EmailInvalid);
        }

        var trimmed = email.Trim();

        if (!EmailRegex.IsMatch(trimmed))
        {
            return Result<Email>.Failure(invalidError ?? SharedErrors.EmailInvalid);
        }

        var normalized = trimmed.ToLowerInvariant();

        if (normalized.Length > MaxLength)
        {
            return Result<Email>.Failure(tooLongError ?? SharedErrors.EmailTooLong);
        }

        return Result<Email>.Success(new Email(normalized));
    }

    public static implicit operator string(Email email) => email.Value;

    public override string ToString() => Value;
}
