using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Application.UnitTests.Fakes;
using NexoFleet.Application.Vehicles;
using NexoFleet.Application.Vehicles.Dtos;
using NexoFleet.Application.Vehicles.Validators;
using NexoFleet.Domain.Common;
using NexoFleet.Domain.Employees;
using NexoFleet.Domain.Vehicles;

namespace NexoFleet.Application.UnitTests.Vehicles;

public sealed class VehicleServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid CompanyId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public async Task RegisterCompanyVehicleAsyncShouldRegisterVehicleWithNotRequiredApproval()
    {
        var repo = new FakeVehicleRepository();
        var empRepo = new FakeEmployeeRepository();
        var uow = new FakeUnitOfWork();
        var tenant = new FakeCurrentTenant(CompanyId);
        var currentUser = new FakeCurrentUser(UserId);
        var clock = new FakeClock(Now);
        var service = CreateService(repo, empRepo, tenant, currentUser, uow, clock);

        var request = new RegisterCompanyVehicleRequest(
            "ABC-123",
            "Toyota",
            "Coaster",
            2022,
            "Blanco",
            VehicleType.Minibus,
            30);

        var result = await service.RegisterCompanyVehicleAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal("ABC-123", result.Value.LicensePlate);
        Assert.Equal(VehicleOwnershipType.CompanyOwned.ToString(), result.Value.OwnershipType);
        Assert.Equal(VehicleApprovalStatus.NotRequired.ToString(), result.Value.ApprovalStatus);
        Assert.True(result.Value.CanOperate);
        Assert.Single(repo.Vehicles);
        Assert.Equal(1, uow.SaveChangesCalls);
    }

    [Fact]
    public async Task RegisterEmployeeVehicleAsyncShouldRequireApproval()
    {
        var repo = new FakeVehicleRepository();
        var empRepo = new FakeEmployeeRepository();
        var uow = new FakeUnitOfWork();
        var tenant = new FakeCurrentTenant(CompanyId);
        var currentUser = new FakeCurrentUser(UserId);
        var clock = new FakeClock(Now);
        var service = CreateService(repo, empRepo, tenant, currentUser, uow, clock);

        var employee = Employee.Create(
            Guid.NewGuid(),
            CompanyId,
            EmployeeCode.Create("EMP-001").Value,
            FullName.Create("Carlos", "Gomez").Value,
            IdentityDocument.Create("V-22334455").Value,
            PhoneNumber.Create("+584141112233").Value,
            Email.Create("carlos@test.com").Value,
            new DateOnly(2025, 1, 1),
            Now).Value;

        empRepo.Employees.Add(employee);

        var request = new RegisterEmployeeVehicleRequest(
            employee.Id,
            "XYZ-789",
            "Chevrolet",
            "Van",
            2020,
            "Gris",
            VehicleType.Van,
            15);

        var result = await service.RegisterEmployeeVehicleAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(VehicleOwnershipType.EmployeeOwned.ToString(), result.Value.OwnershipType);
        Assert.Equal(VehicleApprovalStatus.Pending.ToString(), result.Value.ApprovalStatus);
        Assert.False(result.Value.CanOperate);

        // Approve vehicle
        var approveResult = await service.ApproveAsync(result.Value.Id);
        Assert.True(approveResult.IsSuccess);
        var approvedVehicle = repo.Vehicles.Single(v => v.Id == result.Value.Id);
        Assert.Equal(VehicleApprovalStatus.Approved, approvedVehicle.ApprovalStatus);
        Assert.True(approvedVehicle.CanOperate);
    }

    [Fact]
    public async Task AddAndRemoveDocumentShouldManageDocuments()
    {
        var repo = new FakeVehicleRepository();
        var empRepo = new FakeEmployeeRepository();
        var uow = new FakeUnitOfWork();
        var tenant = new FakeCurrentTenant(CompanyId);
        var currentUser = new FakeCurrentUser(UserId);
        var clock = new FakeClock(Now);
        var service = CreateService(repo, empRepo, tenant, currentUser, uow, clock);

        var vehicle = Vehicle.CreateCompanyOwned(
            Guid.NewGuid(),
            CompanyId,
            "DOC-123",
            "Toyota",
            "HiAce",
            2021,
            "Blanco",
            VehicleType.Van,
            14,
            Now).Value;

        repo.Vehicles.Add(vehicle);

        var addDocRequest = new AddVehicleDocumentRequest(
            VehicleDocumentType.Insurance,
            "poliza_2026.pdf",
            "docs/vehicles/poliza_2026.pdf",
            "application/pdf",
            102400,
            new DateOnly(2027, 1, 1));

        var addResult = await service.AddDocumentAsync(vehicle.Id, addDocRequest);
        Assert.True(addResult.IsSuccess);
        Assert.Single(addResult.Value.Documents);

        var docId = addResult.Value.Documents[0].Id;
        var removeResult = await service.RemoveDocumentAsync(vehicle.Id, docId);
        Assert.True(removeResult.IsSuccess);
        Assert.Empty(vehicle.Documents);
    }

    [Fact]
    public async Task MaintenanceLifecycleShouldTransitionCorrectly()
    {
        var repo = new FakeVehicleRepository();
        var empRepo = new FakeEmployeeRepository();
        var uow = new FakeUnitOfWork();
        var tenant = new FakeCurrentTenant(CompanyId);
        var currentUser = new FakeCurrentUser(UserId);
        var clock = new FakeClock(Now);
        var service = CreateService(repo, empRepo, tenant, currentUser, uow, clock);

        var vehicle = Vehicle.CreateCompanyOwned(
            Guid.NewGuid(),
            CompanyId,
            "MNT-123",
            "Toyota",
            "HiAce",
            2021,
            "Blanco",
            VehicleType.Van,
            14,
            Now).Value;

        repo.Vehicles.Add(vehicle);

        var toMaintenance = await service.SendToMaintenanceAsync(vehicle.Id);
        Assert.True(toMaintenance.IsSuccess);
        Assert.Equal(VehicleStatus.Maintenance, vehicle.Status);
        Assert.False(vehicle.CanOperate);

        var toOperational = await service.ReturnToOperationalAsync(vehicle.Id);
        Assert.True(toOperational.IsSuccess);
        Assert.Equal(VehicleStatus.Operational, vehicle.Status);
        Assert.True(vehicle.CanOperate);

        var toRetired = await service.RetireAsync(vehicle.Id);
        Assert.True(toRetired.IsSuccess);
        Assert.Equal(VehicleStatus.Retired, vehicle.Status);
    }

    private static VehicleService CreateService(
        FakeVehicleRepository repo,
        FakeEmployeeRepository empRepo,
        FakeCurrentTenant? tenant = null,
        FakeCurrentUser? currentUser = null,
        FakeUnitOfWork? uow = null,
        FakeClock? clock = null)
    {
        return new VehicleService(
            repo,
            empRepo,
            tenant ?? new FakeCurrentTenant(CompanyId),
            currentUser ?? new FakeCurrentUser(UserId),
            uow ?? new FakeUnitOfWork(),
            clock ?? new FakeClock(Now),
            new RegisterCompanyVehicleRequestValidator(),
            new RegisterEmployeeVehicleRequestValidator(),
            new UpdateVehicleDetailsRequestValidator(),
            new RejectVehicleRequestValidator(),
            new AddVehicleDocumentRequestValidator());
    }
}
