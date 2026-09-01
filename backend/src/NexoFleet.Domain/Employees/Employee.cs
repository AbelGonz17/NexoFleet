using NexoFleet.Domain.Common;
using NexoFleet.Domain.Employees.Events;

namespace NexoFleet.Domain.Employees;

public sealed class Employee : AggregateRoot
{
    private Employee(
        Guid id,
        Guid companyId,
        EmployeeCode employeeCode,
        FullName fullName,
        IdentityDocument identityDocument,
        PhoneNumber phone,
        Email email,
        DateOnly hireDate,
        DateTimeOffset createdAtUtc) : base(id)
    {
        CompanyId = companyId;
        EmployeeCode = employeeCode;
        FullName = fullName;
        IdentityDocument = identityDocument;
        Phone = phone;
        Email = email;
        HireDate = hireDate;
        Status = EmployeeStatus.Active;
        CreatedAtUtc = createdAtUtc;
    }

    private Employee()
    {
    }

    public Guid CompanyId { get; private set; }

    public Guid? UserId { get; private set; }

    public EmployeeCode EmployeeCode { get; private set; } = null!;

    public FullName FullName { get; private set; } = null!;

    public IdentityDocument IdentityDocument { get; private set; } = null!;

    public PhoneNumber Phone { get; private set; } = null!;

    public Email Email { get; private set; } = null!;

    public DateOnly HireDate { get; private set; }

    public EmployeeStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public static Result<Employee> Create(
        Guid id,
        Guid companyId,
        EmployeeCode employeeCode,
        FullName fullName,
        IdentityDocument identityDocument,
        PhoneNumber phone,
        Email email,
        DateOnly hireDate,
        DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            return Result<Employee>.Failure(EmployeeErrors.InvalidId);
        }

        if (companyId == Guid.Empty)
        {
            return Result<Employee>.Failure(EmployeeErrors.InvalidCompanyId);
        }

        ArgumentNullException.ThrowIfNull(employeeCode);
        ArgumentNullException.ThrowIfNull(fullName);
        ArgumentNullException.ThrowIfNull(identityDocument);
        ArgumentNullException.ThrowIfNull(phone);
        ArgumentNullException.ThrowIfNull(email);

        if (hireDate > DateOnly.FromDateTime(createdAtUtc.UtcDateTime))
        {
            return Result<Employee>.Failure(EmployeeErrors.HireDateInFuture);
        }

        var employee = new Employee(
            id,
            companyId,
            employeeCode,
            fullName,
            identityDocument,
            phone,
            email,
            hireDate,
            createdAtUtc);

        employee.RaiseDomainEvent(new EmployeeCreatedDomainEvent(
            employee.Id,
            employee.CompanyId,
            createdAtUtc));

        return Result<Employee>.Success(employee);
    }

    public Result UpdateProfile(
        EmployeeCode employeeCode,
        FullName fullName,
        IdentityDocument identityDocument,
        PhoneNumber phone,
        Email email,
        DateOnly hireDate,
        DateTimeOffset updatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(employeeCode);
        ArgumentNullException.ThrowIfNull(fullName);
        ArgumentNullException.ThrowIfNull(identityDocument);
        ArgumentNullException.ThrowIfNull(phone);
        ArgumentNullException.ThrowIfNull(email);

        if (hireDate > DateOnly.FromDateTime(updatedAtUtc.UtcDateTime))
        {
            return Result.Failure(EmployeeErrors.HireDateInFuture);
        }

        if (EmployeeCode == employeeCode &&
            FullName == fullName &&
            IdentityDocument == identityDocument &&
            Phone == phone &&
            Email == email &&
            HireDate == hireDate)
        {
            return Result.Success();
        }

        EmployeeCode = employeeCode;
        FullName = fullName;
        IdentityDocument = identityDocument;
        Phone = phone;
        Email = email;
        HireDate = hireDate;
        UpdatedAtUtc = updatedAtUtc;

        return Result.Success();
    }

    public Result LinkUserAccount(Guid userId, DateTimeOffset updatedAtUtc)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure(EmployeeErrors.InvalidUserId);
        }

        if (UserId == userId)
        {
            return Result.Success();
        }

        if (UserId.HasValue)
        {
            return Result.Failure(EmployeeErrors.UserAccountAlreadyLinked);
        }

        UserId = userId;
        UpdatedAtUtc = updatedAtUtc;
        return Result.Success();
    }

    public Result UnlinkUserAccount(DateTimeOffset updatedAtUtc)
    {
        if (!UserId.HasValue)
        {
            return Result.Failure(EmployeeErrors.UserAccountNotLinked);
        }

        UserId = null;
        UpdatedAtUtc = updatedAtUtc;
        return Result.Success();
    }

    public Result Suspend(DateTimeOffset occurredAtUtc)
    {
        if (Status == EmployeeStatus.Retired)
        {
            return Result.Failure(EmployeeErrors.RetiredStatusIsFinal);
        }

        if (Status == EmployeeStatus.Suspended)
        {
            return Result.Failure(EmployeeErrors.AlreadySuspended);
        }

        ChangeStatus(EmployeeStatus.Suspended, occurredAtUtc);
        return Result.Success();
    }

    public Result Activate(DateTimeOffset occurredAtUtc)
    {
        if (Status == EmployeeStatus.Retired)
        {
            return Result.Failure(EmployeeErrors.RetiredStatusIsFinal);
        }

        if (Status == EmployeeStatus.Active)
        {
            return Result.Failure(EmployeeErrors.AlreadyActive);
        }

        ChangeStatus(EmployeeStatus.Active, occurredAtUtc);
        return Result.Success();
    }

    public Result Retire(DateTimeOffset occurredAtUtc)
    {
        if (Status == EmployeeStatus.Retired)
        {
            return Result.Failure(EmployeeErrors.AlreadyRetired);
        }

        ChangeStatus(EmployeeStatus.Retired, occurredAtUtc);
        return Result.Success();
    }

    private void ChangeStatus(EmployeeStatus newStatus, DateTimeOffset occurredAtUtc)
    {
        var previousStatus = Status;
        Status = newStatus;
        UpdatedAtUtc = occurredAtUtc;

        RaiseDomainEvent(new EmployeeStatusChangedDomainEvent(
            Id,
            CompanyId,
            previousStatus,
            newStatus,
            occurredAtUtc));
    }
}
