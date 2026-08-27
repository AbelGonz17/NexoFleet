using NexoFleet.Domain.Common;
using NexoFleet.Domain.Employees;
using NexoFleet.Domain.Employees.Events;

namespace NexoFleet.Domain.UnitTests.Employees;

public sealed class EmployeeTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 27, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly HireDate = new(2026, 1, 15);

    [Fact]
    public void CreateShouldNormalizeDataAndRaiseDomainEvent()
    {
        var id = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var result = CreateEmployee(id, companyId);

        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value.Id);
        Assert.Equal(companyId, result.Value.CompanyId);
        Assert.Equal("EMP-001", result.Value.EmployeeCode);
        Assert.Equal("Abel", result.Value.FirstName);
        Assert.Equal("González", result.Value.LastName);
        Assert.Equal("CI-123456", result.Value.IdentityDocument);
        Assert.Equal("abel@nexo.test", result.Value.Email);
        Assert.True(result.Value.UsesOwnVehicle);
        Assert.Equal(EmployeeStatus.Active, result.Value.Status);
        Assert.Equal(Now, result.Value.CreatedAtUtc);

        var domainEvent = Assert.IsType<EmployeeCreatedDomainEvent>(
            result.Value.DomainEvents.Single());
        Assert.Equal(id, domainEvent.EmployeeId);
        Assert.Equal(companyId, domainEvent.CompanyId);
    }

    [Theory]
    [InlineData("", "Abel", "González", "CI-123", "+59170000000", "abel@nexo.test", "Employee.EmployeeCodeRequired")]
    [InlineData("EMP-001", "", "González", "CI-123", "+59170000000", "abel@nexo.test", "Employee.FirstNameRequired")]
    [InlineData("EMP-001", "Abel", "", "CI-123", "+59170000000", "abel@nexo.test", "Employee.LastNameRequired")]
    [InlineData("EMP-001", "Abel", "González", "", "+59170000000", "abel@nexo.test", "Employee.IdentityDocumentRequired")]
    [InlineData("EMP-001", "Abel", "González", "CI-123", "", "abel@nexo.test", "Employee.PhoneRequired")]
    [InlineData("EMP-001", "Abel", "González", "CI-123", "+59170000000", "invalid", "Employee.EmailInvalid")]
    public void CreateShouldRejectInvalidProfile(
        string employeeCode,
        string firstName,
        string lastName,
        string identityDocument,
        string phone,
        string email,
        string expectedErrorCode)
    {
        var result = Employee.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            employeeCode,
            firstName,
            lastName,
            identityDocument,
            phone,
            email,
            HireDate,
            false,
            Now);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedErrorCode, result.Error.Code);
    }

    [Fact]
    public void CreateShouldRejectFutureHireDate()
    {
        var result = Employee.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "EMP-001",
            "Abel",
            "González",
            "CI-123",
            "+59170000000",
            "abel@nexo.test",
            DateOnly.FromDateTime(Now.UtcDateTime).AddDays(1),
            false,
            Now);

        Assert.True(result.IsFailure);
        Assert.Equal(EmployeeErrors.HireDateInFuture, result.Error);
    }

    [Fact]
    public void UpdateProfileShouldChangeAndNormalizeData()
    {
        var employee = CreateEmployee().Value;
        employee.ClearDomainEvents();

        var result = employee.UpdateProfile(
            " emp-002 ",
            " María ",
            " Pérez ",
            " ci-987 ",
            " +59171111111 ",
            " MARIA@NEXO.TEST ",
            HireDate.AddMonths(1),
            Now.AddHours(1));

        Assert.True(result.IsSuccess);
        Assert.Equal("EMP-002", employee.EmployeeCode);
        Assert.Equal("María", employee.FirstName);
        Assert.Equal("CI-987", employee.IdentityDocument);
        Assert.Equal("maria@nexo.test", employee.Email);
        Assert.Equal(Now.AddHours(1), employee.UpdatedAtUtc);
        Assert.Empty(employee.DomainEvents);
    }

    [Fact]
    public void UpdateProfileShouldNotUpdateTimestampWhenNothingChanged()
    {
        var employee = CreateEmployee().Value;
        employee.ClearDomainEvents();

        var result = employee.UpdateProfile(
            employee.EmployeeCode,
            employee.FirstName,
            employee.LastName,
            employee.IdentityDocument,
            employee.Phone,
            employee.Email,
            employee.HireDate,
            Now.AddHours(1));

        Assert.True(result.IsSuccess);
        Assert.Null(employee.UpdatedAtUtc);
        Assert.Empty(employee.DomainEvents);
    }

    [Fact]
    public void OwnVehicleUsageShouldOnlyUpdateWhenValueChanges()
    {
        var employee = CreateEmployee().Value;

        var unchangedResult = employee.SetOwnVehicleUsage(true, Now.AddHours(1));
        var changedResult = employee.SetOwnVehicleUsage(false, Now.AddHours(2));

        Assert.True(unchangedResult.IsSuccess);
        Assert.True(changedResult.IsSuccess);
        Assert.False(employee.UsesOwnVehicle);
        Assert.Equal(Now.AddHours(2), employee.UpdatedAtUtc);
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
        Guid? companyId = null) =>
        Employee.Create(
            id ?? Guid.NewGuid(),
            companyId ?? Guid.NewGuid(),
            " emp-001 ",
            " Abel ",
            " González ",
            " ci-123456 ",
            " +59170000000 ",
            " ABEL@NEXO.TEST ",
            HireDate,
            true,
            Now);
}
