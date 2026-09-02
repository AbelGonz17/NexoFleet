using NexoFleet.Application.RouteSchedules;
using NexoFleet.Application.RouteSchedules.Dtos;
using NexoFleet.Application.RouteSchedules.Validators;
using NexoFleet.Application.UnitTests.Fakes;
using NexoFleet.Domain.Common;
using NexoFleet.Domain.Employees;
using NexoFleet.Domain.Routes;
using NexoFleet.Domain.RouteSchedules;
using NexoFleet.Domain.Vehicles;

namespace NexoFleet.Application.UnitTests.RouteSchedules;

public sealed class RouteScheduleServiceTests
{
    private static readonly Guid CompanyId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2026, 3, 1, 10, 0, 0, TimeSpan.Zero);

    private static RouteScheduleService CreateService(
        FakeRouteScheduleRepository scheduleRepo,
        FakeRouteRepository routeRepo,
        FakeEmployeeRepository employeeRepo,
        FakeVehicleRepository vehicleRepo,
        FakeCurrentTenant tenant,
        FakeUnitOfWork uow,
        FakeClock clock)
    {
        return new RouteScheduleService(
            scheduleRepo,
            routeRepo,
            employeeRepo,
            vehicleRepo,
            tenant,
            uow,
            clock,
            new CreateRouteScheduleRequestValidator(),
            new ConfigureRouteScheduleRecurrenceRequestValidator(),
            new AssignScheduleResourcesRequestValidator(),
            new EndCurrentScheduleAssignmentRequestValidator());
    }

    [Fact]
    public async Task CreateAsyncShouldCreateScheduleWhenValid()
    {
        var scheduleRepo = new FakeRouteScheduleRepository();
        var routeRepo = new FakeRouteRepository();
        var employeeRepo = new FakeEmployeeRepository();
        var vehicleRepo = new FakeVehicleRepository();
        var tenant = new FakeCurrentTenant(CompanyId);
        var uow = new FakeUnitOfWork();
        var clock = new FakeClock(Now);
        var service = CreateService(scheduleRepo, routeRepo, employeeRepo, vehicleRepo, tenant, uow, clock);

        var route = Route.Create(
            Guid.NewGuid(),
            CompanyId,
            null,
            "R-001",
            "Ruta Matutina",
            RouteLocation.Create("Origen").Value,
            RouteLocation.Create("Destino").Value,
            null,
            null,
            null,
            null,
            Now).Value;
        routeRepo.Routes.Add(route);

        var request = new CreateRouteScheduleRequest(
            route.Id,
            RouteShift.Morning,
            new TimeOnly(7, 0),
            [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday],
            new DateOnly(2026, 3, 1),
            new TimeOnly(8, 30),
            new DateOnly(2026, 12, 31),
            50.00m,
            "USD");

        var result = await service.CreateAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(RouteShift.Morning.ToString(), result.Value.Shift);
        Assert.Equal(3, result.Value.Days.Count);
        Assert.Single(scheduleRepo.Schedules);
        Assert.Equal(1, uow.SaveChangesCalls);
    }

    [Fact]
    public async Task AssignResourcesAndEndAssignmentShouldTrackHistory()
    {
        var scheduleRepo = new FakeRouteScheduleRepository();
        var routeRepo = new FakeRouteRepository();
        var employeeRepo = new FakeEmployeeRepository();
        var vehicleRepo = new FakeVehicleRepository();
        var tenant = new FakeCurrentTenant(CompanyId);
        var uow = new FakeUnitOfWork();
        var clock = new FakeClock(Now);
        var service = CreateService(scheduleRepo, routeRepo, employeeRepo, vehicleRepo, tenant, uow, clock);

        var schedule = RouteSchedule.Create(
            Guid.NewGuid(),
            CompanyId,
            Guid.NewGuid(),
            RouteShift.Night,
            new TimeOnly(19, 0),
            null,
            [DayOfWeek.Monday, DayOfWeek.Tuesday],
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            null,
            null,
            Now).Value;
        scheduleRepo.Schedules.Add(schedule);

        var employee = Employee.Create(
            Guid.NewGuid(),
            CompanyId,
            EmployeeCode.Create("EMP-001").Value,
            FullName.Create("Juan", "Perez").Value,
            IdentityDocument.Create("V-11223344").Value,
            PhoneNumber.Create("+584141112233").Value,
            Email.Create("juan@test.com").Value,
            new DateOnly(2025, 1, 1),
            Now).Value;
        employeeRepo.Employees.Add(employee);

        var vehicle = Vehicle.CreateCompanyOwned(
            Guid.NewGuid(),
            CompanyId,
            "ABC-123",
            "Toyota",
            "Coaster",
            2022,
            "Blanco",
            VehicleType.Minibus,
            30,
            Now).Value;
        vehicleRepo.Vehicles.Add(vehicle);

        // Assign resources
        var assignRequest = new AssignScheduleResourcesRequest(
            employee.Id,
            new DateOnly(2026, 3, 1),
            vehicle.Id);

        var assignResult = await service.AssignResourcesAsync(schedule.Id, assignRequest);
        Assert.True(assignResult.IsSuccess);
        Assert.Single(assignResult.Value.Assignments);

        // End current assignment
        var endResult = await service.EndCurrentAssignmentAsync(
            schedule.Id,
            new EndCurrentScheduleAssignmentRequest(new DateOnly(2026, 6, 30)));
        Assert.True(endResult.IsSuccess);
        Assert.NotNull(endResult.Value.Assignments[0].ValidUntil);
    }
}
