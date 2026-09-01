using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Clients;

public sealed record ClientName
{
    public const int MaxLength = 200;

    public string Value { get; }

    private ClientName(string value) => Value = value;

    public static Result<ClientName> Create(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<ClientName>.Failure(ClientErrors.NameRequired);
        }

        var trimmed = name.Trim();

        if (trimmed.Length > MaxLength)
        {
            return Result<ClientName>.Failure(ClientErrors.NameTooLong);
        }

        return Result<ClientName>.Success(new ClientName(trimmed));
    }

    public static implicit operator string(ClientName name) => name.Value;

    public override string ToString() => Value;
}
