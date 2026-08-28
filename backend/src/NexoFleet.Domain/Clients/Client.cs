using System.Net.Mail;
using NexoFleet.Domain.Clients.Events;
using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Clients;

public sealed class Client : AggregateRoot
{
    public const int ClientCodeMaxLength = 50;
    public const int NameMaxLength = 200;
    public const int TaxIdentificationMaxLength = 50;
    public const int ContactNameMaxLength = 200;
    public const int PhoneMaxLength = 30;
    public const int EmailMaxLength = 256;

    private Client(
        Guid id,
        Guid companyId,
        string clientCode,
        string name,
        string? taxIdentification,
        string? contactName,
        string? phone,
        string? email,
        DateTimeOffset createdAtUtc) : base(id)
    {
        CompanyId = companyId;
        ClientCode = clientCode;
        Name = name;
        TaxIdentification = taxIdentification;
        ContactName = contactName;
        Phone = phone;
        Email = email;
        Status = ClientStatus.Active;
        CreatedAtUtc = createdAtUtc;
    }

    private Client()
    {
    }

    public Guid CompanyId { get; private set; }
    public string ClientCode { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? TaxIdentification { get; private set; }
    public string? ContactName { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public ClientStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public static Result<Client> Create(
        Guid id,
        Guid companyId,
        string clientCode,
        string name,
        string? taxIdentification,
        string? contactName,
        string? phone,
        string? email,
        DateTimeOffset createdAtUtc)
    {
        var validationResult = Validate(
            id,
            companyId,
            clientCode,
            name,
            taxIdentification,
            contactName,
            phone,
            email);
        if (validationResult.IsFailure)
        {
            return Result<Client>.Failure(validationResult.Error);
        }

        var client = new Client(
            id,
            companyId,
            NormalizeIdentifier(clientCode),
            Normalize(name),
            NormalizeIdentifierOptional(taxIdentification),
            NormalizeOptional(contactName),
            NormalizeOptional(phone),
            NormalizeEmail(email),
            createdAtUtc);

        client.RaiseDomainEvent(new ClientCreatedDomainEvent(id, companyId, createdAtUtc));
        return Result<Client>.Success(client);
    }

    public Result UpdateProfile(
        string clientCode,
        string name,
        string? taxIdentification,
        string? contactName,
        string? phone,
        string? email,
        DateTimeOffset updatedAtUtc)
    {
        var validationResult = Validate(
            Id,
            CompanyId,
            clientCode,
            name,
            taxIdentification,
            contactName,
            phone,
            email);
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        var normalizedCode = NormalizeIdentifier(clientCode);
        var normalizedName = Normalize(name);
        var normalizedTaxId = NormalizeIdentifierOptional(taxIdentification);
        var normalizedContact = NormalizeOptional(contactName);
        var normalizedPhone = NormalizeOptional(phone);
        var normalizedEmail = NormalizeEmail(email);

        if (ClientCode == normalizedCode && Name == normalizedName &&
            TaxIdentification == normalizedTaxId && ContactName == normalizedContact &&
            Phone == normalizedPhone && Email == normalizedEmail)
        {
            return Result.Success();
        }

        ClientCode = normalizedCode;
        Name = normalizedName;
        TaxIdentification = normalizedTaxId;
        ContactName = normalizedContact;
        Phone = normalizedPhone;
        Email = normalizedEmail;
        UpdatedAtUtc = updatedAtUtc;
        return Result.Success();
    }

    public Result Activate(DateTimeOffset occurredAtUtc)
    {
        if (Status == ClientStatus.Active) return Result.Failure(ClientErrors.AlreadyActive);
        ChangeStatus(ClientStatus.Active, occurredAtUtc);
        return Result.Success();
    }

    public Result Deactivate(DateTimeOffset occurredAtUtc)
    {
        if (Status == ClientStatus.Inactive) return Result.Failure(ClientErrors.AlreadyInactive);
        ChangeStatus(ClientStatus.Inactive, occurredAtUtc);
        return Result.Success();
    }

    private static Result Validate(
        Guid id,
        Guid companyId,
        string clientCode,
        string name,
        string? taxIdentification,
        string? contactName,
        string? phone,
        string? email)
    {
        if (id == Guid.Empty) return Result.Failure(ClientErrors.InvalidId);
        if (companyId == Guid.Empty) return Result.Failure(ClientErrors.InvalidCompanyId);
        if (string.IsNullOrWhiteSpace(clientCode)) return Result.Failure(ClientErrors.ClientCodeRequired);
        if (clientCode.Trim().Length > ClientCodeMaxLength) return Result.Failure(ClientErrors.ClientCodeTooLong);
        if (string.IsNullOrWhiteSpace(name)) return Result.Failure(ClientErrors.NameRequired);
        if (name.Trim().Length > NameMaxLength) return Result.Failure(ClientErrors.NameTooLong);
        if (taxIdentification?.Trim().Length > TaxIdentificationMaxLength) return Result.Failure(ClientErrors.TaxIdentificationTooLong);
        if (contactName?.Trim().Length > ContactNameMaxLength) return Result.Failure(ClientErrors.ContactNameTooLong);
        if (phone?.Trim().Length > PhoneMaxLength) return Result.Failure(ClientErrors.PhoneTooLong);
        if (email?.Trim().Length > EmailMaxLength) return Result.Failure(ClientErrors.EmailTooLong);
        if (!string.IsNullOrWhiteSpace(email) && !MailAddress.TryCreate(email.Trim(), out _)) return Result.Failure(ClientErrors.EmailInvalid);
        return Result.Success();
    }

    private void ChangeStatus(ClientStatus newStatus, DateTimeOffset occurredAtUtc)
    {
        var previousStatus = Status;
        Status = newStatus;
        UpdatedAtUtc = occurredAtUtc;
        RaiseDomainEvent(new ClientStatusChangedDomainEvent(Id, CompanyId, previousStatus, newStatus, occurredAtUtc));
    }

    private static string Normalize(string value) => value.Trim();
    private static string NormalizeIdentifier(string value) => value.Trim().ToUpperInvariant();
    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string? NormalizeIdentifierOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();
    private static string? NormalizeEmail(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
}
