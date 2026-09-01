using NexoFleet.Domain.Common;
using NexoFleet.Domain.Companies.Events;

namespace NexoFleet.Domain.Companies;

public sealed class Company : AggregateRoot
{
    private Company(
        Guid id,
        CompanyName name,
        TaxIdentification taxIdentification,
        Address address,
        PhoneNumber phone,
        Email email,
        DateTimeOffset createdAtUtc) : base(id)
    {
        Name = name;
        TaxIdentification = taxIdentification;
        Address = address;
        Phone = phone;
        Email = email;
        Status = CompanyStatus.Active;
        CreatedAtUtc = createdAtUtc;
    }

    private Company()
    {
    }

    public CompanyName Name { get; private set; } = null!;

    public TaxIdentification TaxIdentification { get; private set; } = null!;

    public Address Address { get; private set; } = null!;

    public PhoneNumber Phone { get; private set; } = null!;

    public Email Email { get; private set; } = null!;

    public CompanyStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public static Result<Company> Create(
        Guid id,
        CompanyName name,
        TaxIdentification taxIdentification,
        Address address,
        PhoneNumber phone,
        Email email,
        DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            return Result<Company>.Failure(CompanyErrors.InvalidId);
        }

        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(taxIdentification);
        ArgumentNullException.ThrowIfNull(address);
        ArgumentNullException.ThrowIfNull(phone);
        ArgumentNullException.ThrowIfNull(email);

        var company = new Company(
            id,
            name,
            taxIdentification,
            address,
            phone,
            email,
            createdAtUtc);

        company.RaiseDomainEvent(new CompanyCreatedDomainEvent(company.Id, createdAtUtc));
        return Result<Company>.Success(company);
    }

    public Result UpdateProfile(
        CompanyName name,
        TaxIdentification taxIdentification,
        Address address,
        PhoneNumber phone,
        Email email,
        DateTimeOffset updatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(taxIdentification);
        ArgumentNullException.ThrowIfNull(address);
        ArgumentNullException.ThrowIfNull(phone);
        ArgumentNullException.ThrowIfNull(email);

        if (Name == name &&
            TaxIdentification == taxIdentification &&
            Address == address &&
            Phone == phone &&
            Email == email)
        {
            return Result.Success();
        }

        Name = name;
        TaxIdentification = taxIdentification;
        Address = address;
        Phone = phone;
        Email = email;
        UpdatedAtUtc = updatedAtUtc;

        RaiseDomainEvent(new CompanyProfileUpdatedDomainEvent(Id, updatedAtUtc));
        return Result.Success();
    }

    public Result Suspend(DateTimeOffset occurredAt)
    {
        if (Status == CompanyStatus.Suspended)
        {
            return Result.Failure(CompanyErrors.AlreadySuspended);
        }

        ChangeStatus(CompanyStatus.Suspended, occurredAt);
        return Result.Success();
    }

    public Result Activate(DateTimeOffset occurredAt)
    {
        if (Status == CompanyStatus.Active)
        {
            return Result.Failure(CompanyErrors.AlreadyActive);
        }

        ChangeStatus(CompanyStatus.Active, occurredAt);
        return Result.Success();
    }

    private void ChangeStatus(CompanyStatus newStatus, DateTimeOffset occurredAt)
    {
        var previousStatus = Status;
        Status = newStatus;
        UpdatedAtUtc = occurredAt;
        RaiseDomainEvent(new CompanyStatusChangedDomainEvent(
            Id,
            previousStatus,
            newStatus,
            occurredAt));
    }
}
