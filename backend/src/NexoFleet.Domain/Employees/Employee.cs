using System.Net.Mail;
using NexoFleet.Domain.Common;
using NexoFleet.Domain.Employees.Events;

namespace NexoFleet.Domain.Employees;

public sealed class Employee : AggregateRoot
{
    public const int EmployeeCodeMaxLength = 50;
    public const int FirstNameMaxLength = 100;
    public const int LastNameMaxLength = 100;
    public const int IdentityDocumentMaxLength = 50;
    public const int PhoneMaxLength = 30;
    public const int EmailMaxLength = 256;

    private Employee(
        Guid id,
        Guid companyId,
        string employeeCode,
        string firstName,
        string lastName,
        string identityDocument,
        string phone,
        string email,
        DateOnly hireDate,
        bool usesOwnVehicle,
        DateTimeOffset createdAtUtc) : base(id)
    {
        CompanyId = companyId;
        EmployeeCode = employeeCode;
        FirstName = firstName;
        LastName = lastName;
        IdentityDocument = identityDocument;
        Phone = phone;
        Email = email;
        HireDate = hireDate;
        UsesOwnVehicle = usesOwnVehicle;
        Status = EmployeeStatus.Active;
        CreatedAtUtc = createdAtUtc;
    }

    private Employee()
    {
    }

    public Guid CompanyId { get; private set; }

    public Guid? UserId { get; private set; }

    public string EmployeeCode { get; private set; } = string.Empty;

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string IdentityDocument { get; private set; } = string.Empty;

    public string Phone { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public DateOnly HireDate { get; private set; }

    public bool UsesOwnVehicle { get; private set; }

    public EmployeeStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public static Result<Employee> Create(
        Guid id,
        Guid companyId,
        string employeeCode,
        string firstName,
        string lastName,
        string identityDocument,
        string phone,
        string email,
        DateOnly hireDate,
        bool usesOwnVehicle,
        DateTimeOffset createdAtUtc)
    {
        var validationResult = ValidateProfile(
            id,
            companyId,
            employeeCode,
            firstName,
            lastName,
            identityDocument,
            phone,
            email,
            hireDate,
            createdAtUtc);

        if (validationResult.IsFailure)
        {
            return Result<Employee>.Failure(validationResult.Error);
        }

        var employee = new Employee(
            id,
            companyId,
            NormalizeIdentifier(employeeCode),
            Normalize(firstName),
            Normalize(lastName),
            NormalizeIdentifier(identityDocument),
            Normalize(phone),
            NormalizeEmail(email),
            hireDate,
            usesOwnVehicle,
            createdAtUtc);

        employee.RaiseDomainEvent(new EmployeeCreatedDomainEvent(
            employee.Id,
            employee.CompanyId,
            createdAtUtc));

        return Result<Employee>.Success(employee);
    }

    public Result UpdateProfile(
        string employeeCode,
        string firstName,
        string lastName,
        string identityDocument,
        string phone,
        string email,
        DateOnly hireDate,
        DateTimeOffset updatedAtUtc)
    {
        var validationResult = ValidateProfile(
            Id,
            CompanyId,
            employeeCode,
            firstName,
            lastName,
            identityDocument,
            phone,
            email,
            hireDate,
            updatedAtUtc);

        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        var normalizedEmployeeCode = NormalizeIdentifier(employeeCode);
        var normalizedFirstName = Normalize(firstName);
        var normalizedLastName = Normalize(lastName);
        var normalizedIdentityDocument = NormalizeIdentifier(identityDocument);
        var normalizedPhone = Normalize(phone);
        var normalizedEmail = NormalizeEmail(email);

        if (EmployeeCode == normalizedEmployeeCode &&
            FirstName == normalizedFirstName &&
            LastName == normalizedLastName &&
            IdentityDocument == normalizedIdentityDocument &&
            Phone == normalizedPhone &&
            Email == normalizedEmail &&
            HireDate == hireDate)
        {
            return Result.Success();
        }

        EmployeeCode = normalizedEmployeeCode;
        FirstName = normalizedFirstName;
        LastName = normalizedLastName;
        IdentityDocument = normalizedIdentityDocument;
        Phone = normalizedPhone;
        Email = normalizedEmail;
        HireDate = hireDate;
        UpdatedAtUtc = updatedAtUtc;

        return Result.Success();
    }

    public Result SetOwnVehicleUsage(bool usesOwnVehicle, DateTimeOffset updatedAtUtc)
    {
        if (UsesOwnVehicle == usesOwnVehicle)
        {
            return Result.Success();
        }

        UsesOwnVehicle = usesOwnVehicle;
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

    private static Result ValidateProfile(
        Guid id,
        Guid companyId,
        string employeeCode,
        string firstName,
        string lastName,
        string identityDocument,
        string phone,
        string email,
        DateOnly hireDate,
        DateTimeOffset occurredAtUtc)
    {
        if (id == Guid.Empty) return Result.Failure(EmployeeErrors.InvalidId);
        if (companyId == Guid.Empty) return Result.Failure(EmployeeErrors.InvalidCompanyId);
        if (string.IsNullOrWhiteSpace(employeeCode)) return Result.Failure(EmployeeErrors.EmployeeCodeRequired);
        if (employeeCode.Trim().Length > EmployeeCodeMaxLength) return Result.Failure(EmployeeErrors.EmployeeCodeTooLong);
        if (string.IsNullOrWhiteSpace(firstName)) return Result.Failure(EmployeeErrors.FirstNameRequired);
        if (firstName.Trim().Length > FirstNameMaxLength) return Result.Failure(EmployeeErrors.FirstNameTooLong);
        if (string.IsNullOrWhiteSpace(lastName)) return Result.Failure(EmployeeErrors.LastNameRequired);
        if (lastName.Trim().Length > LastNameMaxLength) return Result.Failure(EmployeeErrors.LastNameTooLong);
        if (string.IsNullOrWhiteSpace(identityDocument)) return Result.Failure(EmployeeErrors.IdentityDocumentRequired);
        if (identityDocument.Trim().Length > IdentityDocumentMaxLength) return Result.Failure(EmployeeErrors.IdentityDocumentTooLong);
        if (string.IsNullOrWhiteSpace(phone)) return Result.Failure(EmployeeErrors.PhoneRequired);
        if (phone.Trim().Length > PhoneMaxLength) return Result.Failure(EmployeeErrors.PhoneTooLong);
        if (string.IsNullOrWhiteSpace(email) || !MailAddress.TryCreate(email.Trim(), out _)) return Result.Failure(EmployeeErrors.EmailInvalid);
        if (email.Trim().Length > EmailMaxLength) return Result.Failure(EmployeeErrors.EmailTooLong);
        if (hireDate > DateOnly.FromDateTime(occurredAtUtc.UtcDateTime)) return Result.Failure(EmployeeErrors.HireDateInFuture);

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

    private static string Normalize(string value) => value.Trim();

    private static string NormalizeIdentifier(string value) =>
        value.Trim().ToUpperInvariant();

    private static string NormalizeEmail(string value) =>
        value.Trim().ToLowerInvariant();
}
