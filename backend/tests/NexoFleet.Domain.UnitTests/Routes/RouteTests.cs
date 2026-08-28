using NexoFleet.Domain.Common;
using NexoFleet.Domain.Routes;
using NexoFleet.Domain.Routes.Events;

namespace NexoFleet.Domain.UnitTests.Routes;

public sealed class RouteTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 28, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateShouldNormalizeDetailsAndRaiseEvent()
    {
        var id = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var clientId = Guid.NewGuid();

        var result = CreateRoute(id, companyId, clientId);

        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value.Id);
        Assert.Equal(companyId, result.Value.CompanyId);
        Assert.Equal(clientId, result.Value.ClientId);
        Assert.Equal("RUTA-001", result.Value.RouteCode);
        Assert.Equal("Ruta Planta Norte", result.Value.Name);
        Assert.Equal("Terminal Central", result.Value.Origin);
        Assert.Equal("Planta Norte", result.Value.Destination);
        Assert.Equal("BOB", result.Value.ReferenceCurrency);
        Assert.Equal(RouteStatus.Active, result.Value.Status);
        Assert.Empty(result.Value.Stops);

        var domainEvent = Assert.IsType<RouteCreatedDomainEvent>(
            result.Value.DomainEvents.Single());
        Assert.Equal(id, domainEvent.RouteId);
        Assert.Equal(companyId, domainEvent.CompanyId);
    }

    [Theory]
    [InlineData("", "Ruta Norte", "Terminal", "Planta", "Route.RouteCodeRequired")]
    [InlineData("R-001", "", "Terminal", "Planta", "Route.NameRequired")]
    [InlineData("R-001", "Ruta Norte", "", "Planta", "Route.OriginRequired")]
    [InlineData("R-001", "Ruta Norte", "Terminal", "", "Route.DestinationRequired")]
    public void CreateShouldRejectRequiredInvalidDetails(
        string routeCode,
        string name,
        string origin,
        string destination,
        string expectedErrorCode)
    {
        var result = Route.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            routeCode,
            name,
            origin,
            destination,
            null,
            null,
            null,
            null,
            Now);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedErrorCode, result.Error.Code);
    }

    [Fact]
    public void CreateShouldRejectInvalidOptionalClientId()
    {
        var result = CreateRoute(clientId: Guid.Empty);

        Assert.True(result.IsFailure);
        Assert.Equal(RouteErrors.InvalidClientId, result.Error);
    }

    [Theory]
    [InlineData(null, "BOB", "Route.ReferenceAmountRequired")]
    [InlineData(150.0, null, "Route.ReferenceCurrencyRequired")]
    [InlineData(150.0, "BOL", null)]
    [InlineData(150.0, "BO", "Route.ReferenceCurrencyInvalid")]
    [InlineData(-1.0, "BOB", "Route.InvalidReferenceAmount")]
    public void ReferenceAmountAndCurrencyShouldBeConsistent(
        double? amount,
        string? currency,
        string? expectedErrorCode)
    {
        var result = Route.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "R-001",
            "Ruta Norte",
            "Terminal",
            "Planta",
            null,
            45,
            amount.HasValue ? (decimal)amount.Value : null,
            currency,
            Now);

        if (expectedErrorCode is null)
        {
            Assert.True(result.IsSuccess);
            Assert.Equal("BOL", result.Value.ReferenceCurrency);
        }
        else
        {
            Assert.True(result.IsFailure);
            Assert.Equal(expectedErrorCode, result.Error.Code);
        }
    }

    [Fact]
    public void UpdateDetailsShouldChangeValuesAndAvoidRedundantUpdates()
    {
        var route = CreateRoute().Value;
        route.ClearDomainEvents();

        var updateResult = route.UpdateDetails(
            null,
            " ruta-002 ",
            " Ruta Sur ",
            " Planta Norte ",
            " Terminal Sur ",
            null,
            60,
            175m,
            " usd ",
            Now.AddHours(1));

        var unchangedResult = route.UpdateDetails(
            route.ClientId,
            route.RouteCode,
            route.Name,
            route.Origin,
            route.Destination,
            route.Instructions,
            route.EstimatedDurationMinutes,
            route.ReferenceAmount,
            route.ReferenceCurrency,
            Now.AddHours(2));

        Assert.True(updateResult.IsSuccess);
        Assert.True(unchangedResult.IsSuccess);
        Assert.Null(route.ClientId);
        Assert.Equal("RUTA-002", route.RouteCode);
        Assert.Equal("USD", route.ReferenceCurrency);
        Assert.Equal(Now.AddHours(1), route.UpdatedAtUtc);
        Assert.Empty(route.DomainEvents);
    }

    [Fact]
    public void StopsShouldBeAddedInOrderAndNormalized()
    {
        var route = CreateRoute().Value;
        var firstStopId = Guid.NewGuid();
        var secondStopId = Guid.NewGuid();

        var firstResult = route.AddStop(
            firstStopId,
            " Avenida Principal 100 ",
            " Frente a la plaza ",
            Now.AddMinutes(1));
        var secondResult = route.AddStop(
            secondStopId,
            " Calle Norte 25 ",
            null,
            Now.AddMinutes(2));

        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsSuccess);
        Assert.Collection(
            route.Stops,
            stop =>
            {
                Assert.Equal(firstStopId, stop.Id);
                Assert.Equal(1, stop.Sequence);
                Assert.Equal("Avenida Principal 100", stop.Address);
                Assert.Equal("Frente a la plaza", stop.Instructions);
            },
            stop =>
            {
                Assert.Equal(secondStopId, stop.Id);
                Assert.Equal(2, stop.Sequence);
            });
    }

    [Fact]
    public void StopIdentifierShouldBeUniqueInsideRoute()
    {
        var route = CreateRoute().Value;
        var stopId = Guid.NewGuid();
        route.AddStop(stopId, "Primera parada", null, Now.AddMinutes(1));

        var result = route.AddStop(stopId, "Otra parada", null, Now.AddMinutes(2));

        Assert.True(result.IsFailure);
        Assert.Equal(RouteErrors.StopAlreadyExists, result.Error);
        Assert.Single(route.Stops);
    }

    [Fact]
    public void StopShouldBeUpdatedWithoutChangingItsSequence()
    {
        var route = CreateRoute().Value;
        var stopId = Guid.NewGuid();
        route.AddStop(stopId, "Dirección inicial", null, Now.AddMinutes(1));

        var result = route.UpdateStop(
            stopId,
            " Dirección corregida ",
            " Portón azul ",
            Now.AddMinutes(2));

        Assert.True(result.IsSuccess);
        var stop = Assert.Single(route.Stops);
        Assert.Equal(1, stop.Sequence);
        Assert.Equal("Dirección corregida", stop.Address);
        Assert.Equal("Portón azul", stop.Instructions);
    }

    [Fact]
    public void MoveAndRemoveStopShouldKeepContinuousSequence()
    {
        var route = CreateRoute().Value;
        var firstStopId = Guid.NewGuid();
        var secondStopId = Guid.NewGuid();
        var thirdStopId = Guid.NewGuid();
        route.AddStop(firstStopId, "Primera", null, Now.AddMinutes(1));
        route.AddStop(secondStopId, "Segunda", null, Now.AddMinutes(2));
        route.AddStop(thirdStopId, "Tercera", null, Now.AddMinutes(3));

        var moveResult = route.MoveStop(thirdStopId, 1, Now.AddMinutes(4));
        var removeResult = route.RemoveStop(firstStopId, Now.AddMinutes(5));

        Assert.True(moveResult.IsSuccess);
        Assert.True(removeResult.IsSuccess);
        Assert.Collection(
            route.Stops,
            stop =>
            {
                Assert.Equal(thirdStopId, stop.Id);
                Assert.Equal(1, stop.Sequence);
            },
            stop =>
            {
                Assert.Equal(secondStopId, stop.Id);
                Assert.Equal(2, stop.Sequence);
            });
    }

    [Fact]
    public void InvalidStopOperationsShouldNotMutateRoute()
    {
        var route = CreateRoute().Value;
        var stopId = Guid.NewGuid();
        route.AddStop(stopId, "Primera", null, Now.AddMinutes(1));
        var previousUpdatedAt = route.UpdatedAtUtc;

        var moveResult = route.MoveStop(stopId, 2, Now.AddMinutes(2));
        var removeResult = route.RemoveStop(Guid.NewGuid(), Now.AddMinutes(3));

        Assert.Equal(RouteErrors.InvalidStopSequence, moveResult.Error);
        Assert.Equal(RouteErrors.StopNotFound, removeResult.Error);
        Assert.Equal(previousUpdatedAt, route.UpdatedAtUtc);
        Assert.Single(route.Stops);
    }

    [Fact]
    public void DeactivateAndActivateShouldPublishStatusEvents()
    {
        var route = CreateRoute().Value;
        route.ClearDomainEvents();

        var deactivateResult = route.Deactivate(Now.AddHours(1));
        var activateResult = route.Activate(Now.AddHours(2));

        Assert.True(deactivateResult.IsSuccess);
        Assert.True(activateResult.IsSuccess);
        Assert.Equal(RouteStatus.Active, route.Status);
        Assert.Equal(2, route.DomainEvents.Count);

        var lastEvent = Assert.IsType<RouteStatusChangedDomainEvent>(
            route.DomainEvents.Last());
        Assert.Equal(RouteStatus.Inactive, lastEvent.PreviousStatus);
        Assert.Equal(RouteStatus.Active, lastEvent.CurrentStatus);
    }

    private static Result<Route> CreateRoute(
        Guid? id = null,
        Guid? companyId = null,
        Guid? clientId = null) =>
        Route.Create(
            id ?? Guid.NewGuid(),
            companyId ?? Guid.NewGuid(),
            clientId,
            " ruta-001 ",
            " Ruta Planta Norte ",
            " Terminal Central ",
            " Planta Norte ",
            " Ingresar por el portón principal ",
            45,
            150m,
            " bob ",
            Now);
}
