using NexoFleet.Application.Trips;
using NexoFleet.Application.Trips.Dtos;
using NexoFleet.Application.Trips.Validators;
using NexoFleet.Application.UnitTests.Fakes;
using NexoFleet.Domain.Common;
using NexoFleet.Domain.Employees;
using NexoFleet.Domain.Routes;
using NexoFleet.Domain.Trips;
using NexoFleet.Domain.Vehicles;

namespace NexoFleet.Application.UnitTests.Trips;

public sealed class TripServiceTests
{
    private static readonly Guid CompanyId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2026, 3, 1, 10, 0, 0, TimeSpan.Zero);

    private static TripService CreateService(
        FakeTripRepository tripRepo,
        FakeClientRepository clientRepo,
        FakeRouteRepository routeRepo,
        FakeRouteScheduleRepository scheduleRepo,
        FakeEmployeeRepository employeeRepo,
        FakeVehicleRepository vehicleRepo,
        FakeCurrentTenant tenant,
        FakeCurrentUser currentUser,
        FakeUnitOfWork uow,
        FakeClock clock)
    {
        return new TripService(
            tripRepo,
            clientRepo,
            routeRepo,
            scheduleRepo,
            employeeRepo,
            vehicleRepo,
            tenant,
            currentUser,
            uow,
            clock,
            new CreatePlannedTripRequestValidator(),
            new SubmitUnexpectedTripRequestValidator(),
            new UpdateTripPlanRequestValidator(),
            new ApproveTripRequestValidator(),
            new RejectTripRequestValidator(),
            new AssignTripRequestValidator(),
            new CompleteTripRequestValidator(),
            new CancelTripRequestValidator(),
            new AddTripIncidentRequestValidator(),
            new AddTripFileRequestValidator());
    }

    [Fact]
    public async Task CreatePlannedAsyncShouldCreatePlannedTrip()
    {
        var tripRepo = new FakeTripRepository();
        var clientRepo = new FakeClientRepository();
        var routeRepo = new FakeRouteRepository();
        var scheduleRepo = new FakeRouteScheduleRepository();
        var employeeRepo = new FakeEmployeeRepository();
        var vehicleRepo = new FakeVehicleRepository();
        var tenant = new FakeCurrentTenant(CompanyId);
        var user = new FakeCurrentUser(UserId);
        var uow = new FakeUnitOfWork();
        var clock = new FakeClock(Now);
        var service = CreateService(tripRepo, clientRepo, routeRepo, scheduleRepo, employeeRepo, vehicleRepo, tenant, user, uow, clock);

        var request = new CreatePlannedTripRequest(
            "TRIP-001",
            new DateOnly(2026, 3, 2),
            new TripLocationDto("Plaza Venezuela", 10.4900m, -66.8800m),
            new TripLocationDto("La Guaira", 10.6000m, -66.9300m),
            AgreedAmount: 200.00m,
            Currency: "USD");

        var result = await service.CreatePlannedAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal("TRIP-001", result.Value.TripNumber);
        Assert.Equal(TripStatus.Planned.ToString(), result.Value.Status);
        Assert.Single(tripRepo.Trips);
        Assert.Equal(1, uow.SaveChangesCalls);
    }

    [Fact]
    public async Task UnexpectedTripLifecycleShouldSubmitApproveAssignStartAndComplete()
    {
        var tripRepo = new FakeTripRepository();
        var clientRepo = new FakeClientRepository();
        var routeRepo = new FakeRouteRepository();
        var scheduleRepo = new FakeRouteScheduleRepository();
        var employeeRepo = new FakeEmployeeRepository();
        var vehicleRepo = new FakeVehicleRepository();
        var tenant = new FakeCurrentTenant(CompanyId);
        var user = new FakeCurrentUser(UserId);
        var uow = new FakeUnitOfWork();
        var clock = new FakeClock(Now);
        var service = CreateService(tripRepo, clientRepo, routeRepo, scheduleRepo, employeeRepo, vehicleRepo, tenant, user, uow, clock);

        var employee = Employee.Create(
            Guid.NewGuid(),
            CompanyId,
            EmployeeCode.Create("EMP-001").Value,
            FullName.Create("Manuel", "Rivas").Value,
            IdentityDocument.Create("V-12345678").Value,
            PhoneNumber.Create("+584141112233").Value,
            Email.Create("manuel@test.com").Value,
            new DateOnly(2025, 1, 1),
            Now).Value;
        employeeRepo.Employees.Add(employee);

        var vehicle = Vehicle.CreateCompanyOwned(
            Guid.NewGuid(),
            CompanyId,
            "VEH-001",
            "Toyota",
            "Coaster",
            2021,
            "Blanco",
            VehicleType.Minibus,
            30,
            Now).Value;
        vehicleRepo.Vehicles.Add(vehicle);

        // 1. Submit Unexpected Trip
        var submitRequest = new SubmitUnexpectedTripRequest(
            "TRIP-UNX-01",
            employee.Id,
            new DateOnly(2026, 3, 1),
            new TripLocationDto("Origen Imprevisto"),
            new TripLocationDto("Destino Imprevisto"),
            ProposedAmount: 80.00m,
            Currency: "USD");

        var submitResult = await service.SubmitUnexpectedAsync(submitRequest);
        Assert.True(submitResult.IsSuccess);
        Assert.Equal(TripStatus.PendingApproval.ToString(), submitResult.Value.Status);

        var tripId = submitResult.Value.Id;

        // 2. Approve Trip
        var approveResult = await service.ApproveAsync(tripId, new ApproveTripRequest("Aprobado para despacho"));
        Assert.True(approveResult.IsSuccess);
        Assert.Equal(TripStatus.Planned.ToString(), approveResult.Value.Status);

        // 3. Assign Driver & Vehicle
        var assignResult = await service.AssignAsync(tripId, new AssignTripRequest(employee.Id, vehicle.Id));
        Assert.True(assignResult.IsSuccess);
        Assert.Equal(TripStatus.Assigned.ToString(), assignResult.Value.Status);

        // 4. Start Trip
        clock.UtcNow = Now.AddHours(1);
        var startResult = await service.StartAsync(tripId, employee.Id);
        Assert.True(startResult.IsSuccess);
        Assert.Equal(TripStatus.InProgress.ToString(), startResult.Value.Status);

        // 5. Complete Trip
        clock.UtcNow = Now.AddHours(3);
        var completeResult = await service.CompleteAsync(tripId, employee.Id, new CompleteTripRequest(85.00m, "USD"));
        Assert.True(completeResult.IsSuccess);
        Assert.Equal(TripStatus.Completed.ToString(), completeResult.Value.Status);
        Assert.Equal(85.00m, completeResult.Value.FinalAmount);
    }

    [Fact]
    public async Task IncidentsAndFilesShouldAttachCorrectly()
    {
        var tripRepo = new FakeTripRepository();
        var clientRepo = new FakeClientRepository();
        var routeRepo = new FakeRouteRepository();
        var scheduleRepo = new FakeRouteScheduleRepository();
        var employeeRepo = new FakeEmployeeRepository();
        var vehicleRepo = new FakeVehicleRepository();
        var tenant = new FakeCurrentTenant(CompanyId);
        var user = new FakeCurrentUser(UserId);
        var uow = new FakeUnitOfWork();
        var clock = new FakeClock(Now);
        var service = CreateService(tripRepo, clientRepo, routeRepo, scheduleRepo, employeeRepo, vehicleRepo, tenant, user, uow, clock);

        var employee = Employee.Create(
            Guid.NewGuid(),
            CompanyId,
            EmployeeCode.Create("EMP-002").Value,
            FullName.Create("Pedro", "Infante").Value,
            IdentityDocument.Create("V-99887766").Value,
            PhoneNumber.Create("+584149998877").Value,
            Email.Create("pedro@test.com").Value,
            new DateOnly(2025, 1, 1),
            Now).Value;
        employeeRepo.Employees.Add(employee);

        var trip = Trip.CreatePlanned(
            Guid.NewGuid(),
            CompanyId,
            "TRIP-INC-01",
            null,
            null,
            null,
            new DateOnly(2026, 3, 1),
            RouteLocation.Create("Origen").Value,
            RouteLocation.Create("Destino").Value,
            null,
            null,
            Now).Value;
        trip.Assign(Guid.NewGuid(), employee.Id, Guid.NewGuid(), UserId, Now);
        tripRepo.Trips.Add(trip);

        // Add incident
        var incidentRequest = new AddTripIncidentRequest(
            employee.Id,
            TripIncidentSeverity.Medium,
            "Neumático pinchado en km 45",
            Now.AddMinutes(30));

        var incidentResult = await service.AddIncidentAsync(trip.Id, incidentRequest);
        Assert.True(incidentResult.IsSuccess);
        Assert.Single(incidentResult.Value.Incidents);

        // Add file
        var fileRequest = new AddTripFileRequest(
            "foto_neumatico.jpg",
            "trips/incidents/foto_neumatico.jpg",
            "image/jpeg",
            204800);

        var fileResult = await service.AddFileAsync(trip.Id, fileRequest);
        Assert.True(fileResult.IsSuccess);
        Assert.Single(fileResult.Value.Files);
    }
}
