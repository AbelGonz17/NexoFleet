using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Clients;

public sealed record ContactName
{
    public const int MaxLength = 200;

    public string Value { get; }

    private ContactName(string value) => Value = value;

    public static Result<ContactName> Create(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<ContactName>.Failure(ClientErrors.ContactNameTooLong);
        }

        var trimmed = name.Trim();

        if (trimmed.Length > MaxLength)
        {
            return Result<ContactName>.Failure(ClientErrors.ContactNameTooLong);
        }

        return Result<ContactName>.Success(new ContactName(trimmed));
    }

    public static implicit operator string(ContactName name) => name.Value;

    public override string ToString() => Value;
}
