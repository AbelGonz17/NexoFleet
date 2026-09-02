using NexoFleet.Application.Routes;
using NexoFleet.Application.Routes.Dtos;
using NexoFleet.Application.Routes.Validators;
using NexoFleet.Application.UnitTests.Fakes;
using NexoFleet.Domain.Routes;

namespace NexoFleet.Application.UnitTests.Routes;

public sealed class RouteServiceTests
{
    private static readonly Guid CompanyId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2026, 3, 1, 10, 0, 0, TimeSpan.Zero);

    private static RouteService CreateService(
        FakeRouteRepository routeRepo,
        FakeClientRepository clientRepo,
        FakeCurrentTenant tenant,
        FakeUnitOfWork uow,
        FakeClock clock)
    {
        return new RouteService(
            routeRepo,
            clientRepo,
            tenant,
            uow,
            clock,
            new CreateRouteRequestValidator(),
            new UpdateRouteDetailsRequestValidator(),
            new AddRouteStopRequestValidator(),
            new UpdateRouteStopRequestValidator());
    }

    [Fact]
    public async Task CreateAsyncShouldCreateRouteWhenRequestIsValid()
    {
        var routeRepo = new FakeRouteRepository();
        var clientRepo = new FakeClientRepository();
        var tenant = new FakeCurrentTenant(CompanyId);
        var uow = new FakeUnitOfWork();
        var clock = new FakeClock(Now);
        var service = CreateService(routeRepo, clientRepo, tenant, uow, clock);

        var request = new CreateRouteRequest(
            "R-001",
            "Ruta Centro - Norte",
            new RouteLocationDto("Av. Principal 123", 10.4806m, -66.9036m),
            new RouteLocationDto("Calle Norte 456", 10.5000m, -66.9100m),
            EstimatedDurationMinutes: 45,
            ReferenceAmount: 150.00m,
            ReferenceCurrency: "USD");

        var result = await service.CreateAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal("R-001", result.Value.RouteCode);
        Assert.Equal("Ruta Centro - Norte", result.Value.Name);
        Assert.Equal("Active", result.Value.Status);
        Assert.Single(routeRepo.Routes);
        Assert.Equal(1, uow.SaveChangesCalls);
    }

    [Fact]
    public async Task CreateAsyncShouldFailWhenRouteCodeIsDuplicate()
    {
        var routeRepo = new FakeRouteRepository();
        var clientRepo = new FakeClientRepository();
        var tenant = new FakeCurrentTenant(CompanyId);
        var uow = new FakeUnitOfWork();
        var clock = new FakeClock(Now);
        var service = CreateService(routeRepo, clientRepo, tenant, uow, clock);

        var existing = Route.Create(
            Guid.NewGuid(),
            CompanyId,
            null,
            "R-001",
            "Ruta Existente",
            RouteLocation.Create("Origen").Value,
            RouteLocation.Create("Destino").Value,
            null,
            30,
            null,
            null,
            Now).Value;
        routeRepo.Routes.Add(existing);

        var request = new CreateRouteRequest(
            "R-001",
            "Ruta Duplicada",
            new RouteLocationDto("Origen 2"),
            new RouteLocationDto("Destino 2"));

        var result = await service.CreateAsync(request);

        Assert.True(result.IsFailure);
        Assert.Equal("Route.RouteCodeDuplicate", result.Error.Code);
    }

    [Fact]
    public async Task StopManagementShouldAddUpdateMoveAndRemoveStops()
    {
        var routeRepo = new FakeRouteRepository();
        var clientRepo = new FakeClientRepository();
        var tenant = new FakeCurrentTenant(CompanyId);
        var uow = new FakeUnitOfWork();
        var clock = new FakeClock(Now);
        var service = CreateService(routeRepo, clientRepo, tenant, uow, clock);

        var route = Route.Create(
            Guid.NewGuid(),
            CompanyId,
            null,
            "R-100",
            "Ruta con Paradas",
            RouteLocation.Create("Terminal Sur").Value,
            RouteLocation.Create("Terminal Norte").Value,
            null,
            60,
            null,
            null,
            Now).Value;
        routeRepo.Routes.Add(route);

        // Add 2 stops
        var stop1Result = await service.AddStopAsync(route.Id, new AddRouteStopRequest(new RouteLocationDto("Parada 1", 10.1m, -66.1m), "Frente a plaza"));
        var stop2Result = await service.AddStopAsync(route.Id, new AddRouteStopRequest(new RouteLocationDto("Parada 2", 10.2m, -66.2m), "Cerca de estación"));

        Assert.True(stop1Result.IsSuccess);
        Assert.True(stop2Result.IsSuccess);
        Assert.Equal(2, stop2Result.Value.Stops.Count);

        var stop1Id = stop2Result.Value.Stops[0].Id;
        var stop2Id = stop2Result.Value.Stops[1].Id;

        // Move Stop 2 to sequence 1
        var moveResult = await service.MoveStopAsync(route.Id, stop2Id, 1);
        Assert.True(moveResult.IsSuccess);
        Assert.Equal(stop2Id, moveResult.Value.Stops[0].Id);
        Assert.Equal(1, moveResult.Value.Stops[0].Sequence);

        // Remove Stop 1
        var removeResult = await service.RemoveStopAsync(route.Id, stop1Id);
        Assert.True(removeResult.IsSuccess);
        Assert.Single(removeResult.Value.Stops);
    }

    [Fact]
    public async Task StatusTransitionsShouldActivateAndDeactivateRoute()
    {
        var routeRepo = new FakeRouteRepository();
        var clientRepo = new FakeClientRepository();
        var tenant = new FakeCurrentTenant(CompanyId);
        var uow = new FakeUnitOfWork();
        var clock = new FakeClock(Now);
        var service = CreateService(routeRepo, clientRepo, tenant, uow, clock);

        var route = Route.Create(
            Guid.NewGuid(),
            CompanyId,
            null,
            "R-200",
            "Ruta Activa",
            RouteLocation.Create("Origen").Value,
            RouteLocation.Create("Destino").Value,
            null,
            null,
            null,
            null,
            Now).Value;
        routeRepo.Routes.Add(route);

        var deactResult = await service.DeactivateAsync(route.Id);
        Assert.True(deactResult.IsSuccess);
        Assert.Equal(RouteStatus.Inactive, route.Status);

        var actResult = await service.ActivateAsync(route.Id);
        Assert.True(actResult.IsSuccess);
        Assert.Equal(RouteStatus.Active, route.Status);
    }
}
