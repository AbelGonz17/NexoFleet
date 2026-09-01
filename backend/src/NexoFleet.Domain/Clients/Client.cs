using NexoFleet.Domain.Clients.Events;
using NexoFleet.Domain.Common;
using NexoFleet.Domain.Companies;

namespace NexoFleet.Domain.Clients;

public sealed class Client : AggregateRoot
{
    private Client(
        Guid id,
        Guid companyId,
        ClientCode clientCode,
        ClientName name,
        TaxIdentification? taxIdentification,
        ContactName? contactName,
        PhoneNumber? phone,
        Email? email,
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

    public ClientCode ClientCode { get; private set; } = null!;

    public ClientName Name { get; private set; } = null!;

    public TaxIdentification? TaxIdentification { get; private set; }

    public ContactName? ContactName { get; private set; }

    public PhoneNumber? Phone { get; private set; }

    public Email? Email { get; private set; }

    public ClientStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public static Result<Client> Create(
        Guid id,
        Guid companyId,
        ClientCode clientCode,
        ClientName name,
        TaxIdentification? taxIdentification,
        ContactName? contactName,
        PhoneNumber? phone,
        Email? email,
        DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            return Result<Client>.Failure(ClientErrors.InvalidId);
        }

        if (companyId == Guid.Empty)
        {
            return Result<Client>.Failure(ClientErrors.InvalidCompanyId);
        }

        ArgumentNullException.ThrowIfNull(clientCode);
        ArgumentNullException.ThrowIfNull(name);

        var client = new Client(
            id,
            companyId,
            clientCode,
            name,
            taxIdentification,
            contactName,
            phone,
            email,
            createdAtUtc);

        client.RaiseDomainEvent(new ClientCreatedDomainEvent(id, companyId, createdAtUtc));
        return Result<Client>.Success(client);
    }

    public Result UpdateProfile(
        ClientCode clientCode,
        ClientName name,
        TaxIdentification? taxIdentification,
        ContactName? contactName,
        PhoneNumber? phone,
        Email? email,
        DateTimeOffset updatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(clientCode);
        ArgumentNullException.ThrowIfNull(name);

        if (ClientCode == clientCode &&
            Name == name &&
            TaxIdentification == taxIdentification &&
            ContactName == contactName &&
            Phone == phone &&
            Email == email)
        {
            return Result.Success();
        }

        ClientCode = clientCode;
        Name = name;
        TaxIdentification = taxIdentification;
        ContactName = contactName;
        Phone = phone;
        Email = email;
        UpdatedAtUtc = updatedAtUtc;

        return Result.Success();
    }

    public Result Activate(DateTimeOffset occurredAtUtc)
    {
        if (Status == ClientStatus.Active)
        {
            return Result.Failure(ClientErrors.AlreadyActive);
        }

        ChangeStatus(ClientStatus.Active, occurredAtUtc);
        return Result.Success();
    }

    public Result Deactivate(DateTimeOffset occurredAtUtc)
    {
        if (Status == ClientStatus.Inactive)
        {
            return Result.Failure(ClientErrors.AlreadyInactive);
        }

        ChangeStatus(ClientStatus.Inactive, occurredAtUtc);
        return Result.Success();
    }

    private void ChangeStatus(ClientStatus newStatus, DateTimeOffset occurredAtUtc)
    {
        var previousStatus = Status;
        Status = newStatus;
        UpdatedAtUtc = occurredAtUtc;
        RaiseDomainEvent(new ClientStatusChangedDomainEvent(Id, CompanyId, previousStatus, newStatus, occurredAtUtc));
    }
}
