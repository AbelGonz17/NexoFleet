using NexoFleet.Domain.Common;
using NexoFleet.Domain.Employees;
using NexoFleet.Domain.Employees.Events;

namespace NexoFleet.Domain.UnitTests.Employees;

public sealed class EmployeeTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 27, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly HireDate = new(2026, 1, 15);

    [Fact]
    public void CreateShouldCreateEmployeeAndRaiseDomainEvent()
    {
        var id = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var result = CreateEmployee(id, companyId);

        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value.Id);
        Assert.Equal(companyId, result.Value.CompanyId);
        Assert.Equal("EMP-001", result.Value.EmployeeCode.Value);
        Assert.Equal("Abel", result.Value.FullName.FirstName);
        Assert.Equal("González", result.Value.FullName.LastName);
        Assert.Equal("CI-123456", result.Value.IdentityDocument.Value);
        Assert.Equal("+59170000000", result.Value.Phone.Value);
        Assert.Equal("abel@nexo.test", result.Value.Email.Value);
        Assert.Equal(EmployeeStatus.Active, result.Value.Status);
        Assert.Equal(Now, result.Value.CreatedAtUtc);

        var domainEvent = Assert.IsType<EmployeeCreatedDomainEvent>(
            result.Value.DomainEvents.Single());
        Assert.Equal(id, domainEvent.EmployeeId);
        Assert.Equal(companyId, domainEvent.CompanyId);
    }

    [Fact]
    public void CreateShouldFailWhenIdIsEmpty()
    {
        var result = Employee.Create(
            Guid.Empty,
            Guid.NewGuid(),
            EmployeeCode.Create("EMP-001").Value,
            FullName.Create("Abel", "González").Value,
            IdentityDocument.Create("CI-123").Value,
            PhoneNumber.Create("+59170000000").Value,
            Email.Create("abel@nexo.test").Value,
            HireDate,
            Now);

        Assert.True(result.IsFailure);
        Assert.Equal(EmployeeErrors.InvalidId, result.Error);
    }

    [Fact]
    public void CreateShouldFailWhenCompanyIdIsEmpty()
    {
        var result = Employee.Create(
            Guid.NewGuid(),
            Guid.Empty,
            EmployeeCode.Create("EMP-001").Value,
            FullName.Create("Abel", "González").Value,
            IdentityDocument.Create("CI-123").Value,
            PhoneNumber.Create("+59170000000").Value,
            Email.Create("abel@nexo.test").Value,
            HireDate,
            Now);

        Assert.True(result.IsFailure);
        Assert.Equal(EmployeeErrors.InvalidCompanyId, result.Error);
    }

    [Fact]
    public void CreateShouldRejectFutureHireDate()
    {
        var result = Employee.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EmployeeCode.Create("EMP-001").Value,
            FullName.Create("Abel", "González").Value,
            IdentityDocument.Create("CI-123").Value,
            PhoneNumber.Create("+59170000000").Value,
            Email.Create("abel@nexo.test").Value,
            DateOnly.FromDateTime(Now.UtcDateTime).AddDays(1),
            Now);

        Assert.True(result.IsFailure);
        Assert.Equal(EmployeeErrors.HireDateInFuture, result.Error);
    }

    [Fact]
    public void UpdateProfileShouldChangeData()
    {
        var employee = CreateEmployee().Value;
        employee.ClearDomainEvents();

        var newCode = EmployeeCode.Create("emp-002").Value;
        var newName = FullName.Create("María", "Pérez").Value;
        var newDoc = IdentityDocument.Create("ci-987").Value;
        var newPhone = PhoneNumber.Create("+59171111111").Value;
        var newEmail = Email.Create("MARIA@NEXO.TEST").Value;
        var newHireDate = HireDate.AddMonths(1);

        var result = employee.UpdateProfile(
            newCode,
            newName,
            newDoc,
            newPhone,
            newEmail,
            newHireDate,
            Now.AddHours(1));

        Assert.True(result.IsSuccess);
        Assert.Equal("EMP-002", employee.EmployeeCode.Value);
        Assert.Equal("María", employee.FullName.FirstName);
        Assert.Equal("Pérez", employee.FullName.LastName);
        Assert.Equal("CI-987", employee.IdentityDocument.Value);
        Assert.Equal("+59171111111", employee.Phone.Value);
        Assert.Equal("maria@nexo.test", employee.Email.Value);
        Assert.Equal(Now.AddHours(1), employee.UpdatedAtUtc);
        Assert.Empty(employee.DomainEvents);
    }

    [Fact]
    public void UpdateProfileShouldNotUpdateTimestampWhenNothingChanged()
    {
        var employee = CreateEmployee().Value;
        employee.ClearDomainEvents();

        var sameCode = EmployeeCode.Create("EMP-001").Value;
        var sameName = FullName.Create("Abel", "González").Value;
        var sameDoc = IdentityDocument.Create("CI-123456").Value;
        var samePhone = PhoneNumber.Create("+59170000000").Value;
        var sameEmail = Email.Create("abel@nexo.test").Value;

        var result = employee.UpdateProfile(
            sameCode,
            sameName,
            sameDoc,
            samePhone,
            sameEmail,
            employee.HireDate,
            Now.AddHours(1));

        Assert.True(result.IsSuccess);
        Assert.Null(employee.UpdatedAtUtc);
        Assert.Empty(employee.DomainEvents);
    }

    [Fact]
    public void UserAccountShouldBeLinkedOnlyOnce()
    {
        var employee = CreateEmployee().Value;
        var userId = Guid.NewGuid();

        var linkResult = employee.LinkUserAccount(userId, Now.AddHours(1));
        var repeatedResult = employee.LinkUserAccount(userId, Now.AddHours(2));
        var otherUserResult = employee.LinkUserAccount(Guid.NewGuid(), Now.AddHours(3));

        Assert.True(linkResult.IsSuccess);
        Assert.True(repeatedResult.IsSuccess);
        Assert.True(otherUserResult.IsFailure);
        Assert.Equal(EmployeeErrors.UserAccountAlreadyLinked, otherUserResult.Error);
        Assert.Equal(userId, employee.UserId);
        Assert.Equal(Now.AddHours(1), employee.UpdatedAtUtc);
    }

    [Fact]
    public void UnlinkUserAccountShouldRequireAnExistingLink()
    {
        var employee = CreateEmployee().Value;

        var missingLinkResult = employee.UnlinkUserAccount(Now.AddHours(1));
        employee.LinkUserAccount(Guid.NewGuid(), Now.AddHours(2));
        var unlinkResult = employee.UnlinkUserAccount(Now.AddHours(3));

        Assert.True(missingLinkResult.IsFailure);
        Assert.Equal(EmployeeErrors.UserAccountNotLinked, missingLinkResult.Error);
        Assert.True(unlinkResult.IsSuccess);
        Assert.Null(employee.UserId);
        Assert.Equal(Now.AddHours(3), employee.UpdatedAtUtc);
    }

    [Fact]
    public void SuspendAndActivateShouldPublishStatusEvents()
    {
        var employee = CreateEmployee().Value;
        employee.ClearDomainEvents();

        var suspendResult = employee.Suspend(Now.AddHours(1));
        var activateResult = employee.Activate(Now.AddHours(2));

        Assert.True(suspendResult.IsSuccess);
        Assert.True(activateResult.IsSuccess);
        Assert.Equal(EmployeeStatus.Active, employee.Status);
        Assert.Equal(2, employee.DomainEvents.Count);

        var lastEvent = Assert.IsType<EmployeeStatusChangedDomainEvent>(
            employee.DomainEvents.Last());
        Assert.Equal(EmployeeStatus.Suspended, lastEvent.PreviousStatus);
        Assert.Equal(EmployeeStatus.Active, lastEvent.CurrentStatus);
    }

    [Fact]
    public void RetiredEmployeeShouldNotReturnToAnotherStatus()
    {
        var employee = CreateEmployee().Value;

        var retireResult = employee.Retire(Now.AddHours(1));
        var activateResult = employee.Activate(Now.AddHours(2));
        var suspendResult = employee.Suspend(Now.AddHours(3));

        Assert.True(retireResult.IsSuccess);
        Assert.Equal(EmployeeStatus.Retired, employee.Status);
        Assert.Equal(EmployeeErrors.RetiredStatusIsFinal, activateResult.Error);
        Assert.Equal(EmployeeErrors.RetiredStatusIsFinal, suspendResult.Error);
    }

    private static Result<Employee> CreateEmployee(
        Guid? id = null,
        Guid? companyId = null)
    {
        var code = EmployeeCode.Create(" emp-001 ").Value;
        var name = FullName.Create(" Abel ", " González ").Value;
        var doc = IdentityDocument.Create(" ci-123456 ").Value;
        var phone = PhoneNumber.Create(" +59170000000 ").Value;
        var email = Email.Create(" ABEL@NEXO.TEST ").Value;

        return Employee.Create(
            id ?? Guid.NewGuid(),
            companyId ?? Guid.NewGuid(),
            code,
            name,
            doc,
            phone,
            email,
            HireDate,
            Now);
    }
}
