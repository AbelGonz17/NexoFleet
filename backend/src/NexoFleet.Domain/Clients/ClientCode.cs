using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Clients;

public sealed record ClientCode
{
    public const int MaxLength = 50;

    public string Value { get; }

    private ClientCode(string value) => Value = value;

    public static Result<ClientCode> Create(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result<ClientCode>.Failure(ClientErrors.ClientCodeRequired);
        }

        var normalized = code.Trim().ToUpperInvariant();

        if (normalized.Length > MaxLength)
        {
            return Result<ClientCode>.Failure(ClientErrors.ClientCodeTooLong);
        }

        return Result<ClientCode>.Success(new ClientCode(normalized));
    }

    public static implicit operator string(ClientCode code) => code.Value;

    public override string ToString() => Value;
}
