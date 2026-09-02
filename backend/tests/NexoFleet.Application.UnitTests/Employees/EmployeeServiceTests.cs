using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Application.Employees;
using NexoFleet.Application.Employees.Dtos;
using NexoFleet.Application.Employees.Validators;
using NexoFleet.Application.UnitTests.Fakes;
using NexoFleet.Domain.Common;
using NexoFleet.Domain.Employees;

namespace NexoFleet.Application.UnitTests.Employees;

public sealed class EmployeeServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid CompanyId = Guid.NewGuid();

    [Fact]
    public async Task CreateAsyncWithValidRequestShouldCreateEmployee()
    {
        var repo = new FakeEmployeeRepository();
        var uow = new FakeUnitOfWork();
        var tenant = new FakeCurrentTenant(CompanyId);
        var clock = new FakeClock(Now);
        var service = CreateService(repo, tenant, uow, clock);

        var request = new CreateEmployeeRequest(
            "EMP-001",
            "Juan",
            "Perez",
            "V-12345678",
            "+584121234567",
            "juan.perez@nexofleet.test",
            new DateOnly(2025, 1, 15));

        var result = await service.CreateAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal("EMP-001", result.Value.EmployeeCode);
        Assert.Equal("Juan Perez", result.Value.FullName);
        Assert.Equal("V-12345678", result.Value.IdentityDocument);
        Assert.Equal(EmployeeStatus.Active.ToString(), result.Value.Status);
        Assert.Single(repo.Employees);
        Assert.Equal(1, uow.SaveChangesCalls);
    }

    [Fact]
    public async Task CreateAsyncWithDuplicateDocumentShouldReturnConflictError()
    {
        var repo = new FakeEmployeeRepository();
        var service = CreateService(repo);

        var existing = Employee.Create(
            Guid.NewGuid(),
            CompanyId,
            EmployeeCode.Create("EMP-001").Value,
            FullName.Create("Juan", "Perez").Value,
            IdentityDocument.Create("V-12345678").Value,
            PhoneNumber.Create("+584121234567").Value,
            Email.Create("juan@test.com").Value,
            new DateOnly(2025, 1, 1),
            Now).Value;

        repo.Employees.Add(existing);

        var request = new CreateEmployeeRequest(
            "EMP-002",
            "Pedro",
            "Gomez",
            "V-12345678",
            "+584129999999",
            "pedro@test.com",
            new DateOnly(2025, 2, 1));

        var result = await service.CreateAsync(request);

        Assert.True(result.IsFailure);
        Assert.Equal(EmployeeErrors.IdentityDocumentDuplicate, result.Error);
    }

    [Fact]
    public async Task LinkAndUnlinkUserAccountShouldUpdateUserId()
    {
        var repo = new FakeEmployeeRepository();
        var uow = new FakeUnitOfWork();
        var service = CreateService(repo, uow: uow);

        var employee = Employee.Create(
            Guid.NewGuid(),
            CompanyId,
            EmployeeCode.Create("EMP-001").Value,
            FullName.Create("Juan", "Perez").Value,
            IdentityDocument.Create("V-12345678").Value,
            PhoneNumber.Create("+584121234567").Value,
            Email.Create("juan@test.com").Value,
            new DateOnly(2025, 1, 1),
            Now).Value;

        repo.Employees.Add(employee);

        var userId = Guid.NewGuid();
        var linkResult = await service.LinkUserAccountAsync(employee.Id, new LinkUserAccountRequest(userId));
        Assert.True(linkResult.IsSuccess);
        Assert.Equal(userId, employee.UserId);

        var unlinkResult = await service.UnlinkUserAccountAsync(employee.Id);
        Assert.True(unlinkResult.IsSuccess);
        Assert.Null(employee.UserId);
    }

    [Fact]
    public async Task SuspendAndRetireShouldTransitionLifecycle()
    {
        var repo = new FakeEmployeeRepository();
        var service = CreateService(repo);

        var employee = Employee.Create(
            Guid.NewGuid(),
            CompanyId,
            EmployeeCode.Create("EMP-001").Value,
            FullName.Create("Juan", "Perez").Value,
            IdentityDocument.Create("V-12345678").Value,
            PhoneNumber.Create("+584121234567").Value,
            Email.Create("juan@test.com").Value,
            new DateOnly(2025, 1, 1),
            Now).Value;

        repo.Employees.Add(employee);

        var suspendResult = await service.SuspendAsync(employee.Id);
        Assert.True(suspendResult.IsSuccess);
        Assert.Equal(EmployeeStatus.Suspended, employee.Status);

        var activateResult = await service.ActivateAsync(employee.Id);
        Assert.True(activateResult.IsSuccess);
        Assert.Equal(EmployeeStatus.Active, employee.Status);

        var retireResult = await service.RetireAsync(employee.Id);
        Assert.True(retireResult.IsSuccess);
        Assert.Equal(EmployeeStatus.Retired, employee.Status);

        // Cannot reactivate retired employee
        var reactivateResult = await service.ActivateAsync(employee.Id);
        Assert.True(reactivateResult.IsFailure);
        Assert.Equal(EmployeeErrors.RetiredStatusIsFinal, reactivateResult.Error);
    }

    private static EmployeeService CreateService(
        FakeEmployeeRepository repo,
        FakeCurrentTenant? tenant = null,
        FakeUnitOfWork? uow = null,
        FakeClock? clock = null)
    {
        return new EmployeeService(
            repo,
            tenant ?? new FakeCurrentTenant(CompanyId),
            uow ?? new FakeUnitOfWork(),
            clock ?? new FakeClock(Now),
            new CreateEmployeeRequestValidator(),
            new UpdateEmployeeRequestValidator(),
            new LinkUserAccountRequestValidator());
    }
}
