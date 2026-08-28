using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using NexoFleet.Domain.Companies;
using NexoFleet.Domain.Routes;
using NexoFleet.Infrastructure.Persistence;

namespace NexoFleet.Infrastructure.UnitTests.Persistence;

public sealed class RouteConfigurationTests
{
    [Fact]
    public void RouteShouldHaveTheExpectedDatabaseConfiguration()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=nexofleet;Username=test;Password=test")
            .Options;
        using var context = new ApplicationDbContext(options);

        var routeEntity = context.Model.FindEntityType(typeof(Route));

        Assert.NotNull(routeEntity);
        Assert.Equal("routes", routeEntity.GetTableName());
        Assert.Equal(
            Route.RouteCodeMaxLength,
            routeEntity.FindProperty(nameof(Route.RouteCode))?.GetMaxLength());
        Assert.Equal(
            Route.CurrencyLength,
            routeEntity.FindProperty(nameof(Route.ReferenceCurrency))?.GetMaxLength());
        Assert.Equal(
            18,
            routeEntity.FindProperty(nameof(Route.ReferenceAmount))?.GetPrecision());
        Assert.Equal(
            2,
            routeEntity.FindProperty(nameof(Route.ReferenceAmount))?.GetScale());
        Assert.Equal(
            typeof(string),
            routeEntity.FindProperty(nameof(Route.Status))?.GetProviderClrType());

        var routeTable = StoreObjectIdentifier.Table("routes", null);
        var origin = routeEntity.FindComplexProperty(nameof(Route.Origin));
        Assert.NotNull(origin);
        Assert.True(origin.IsNullable is false);
        Assert.Equal(
            "origin_address",
            origin.ComplexType.FindProperty(nameof(RouteLocation.Address))?.GetColumnName(routeTable));
        Assert.Equal(
            RouteLocation.AddressMaxLength,
            origin.ComplexType.FindProperty(nameof(RouteLocation.Address))?.GetMaxLength());
        Assert.Equal(
            RouteLocation.CoordinatePrecision,
            origin.ComplexType.FindProperty(nameof(RouteLocation.Latitude))?.GetPrecision());
        Assert.Equal(
            RouteLocation.CoordinateScale,
            origin.ComplexType.FindProperty(nameof(RouteLocation.Longitude))?.GetScale());

        var destination = routeEntity.FindComplexProperty(nameof(Route.Destination));
        Assert.NotNull(destination);
        Assert.Equal(
            "destination_address",
            destination.ComplexType.FindProperty(nameof(RouteLocation.Address))?.GetColumnName(routeTable));

        var routeCodeIndex = routeEntity.GetIndexes().Single(index =>
            index.Properties.Count == 2 &&
            index.Properties[0].Name == nameof(Route.CompanyId) &&
            index.Properties[1].Name == nameof(Route.RouteCode));
        Assert.True(routeCodeIndex.IsUnique);

        var companyForeignKey = routeEntity.GetForeignKeys().Single(foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(Company));
        Assert.Equal(DeleteBehavior.Restrict, companyForeignKey.DeleteBehavior);

        var stopEntity = context.Model.FindEntityType(typeof(RouteStop));
        Assert.NotNull(stopEntity);
        Assert.Equal("route_stops", stopEntity.GetTableName());

        var stopTable = StoreObjectIdentifier.Table("route_stops", null);
        var stopLocation = stopEntity.FindComplexProperty(nameof(RouteStop.Location));
        Assert.NotNull(stopLocation);
        Assert.True(stopLocation.IsNullable is false);
        Assert.Equal(
            "address",
            stopLocation.ComplexType.FindProperty(nameof(RouteLocation.Address))?.GetColumnName(stopTable));
        Assert.Equal(
            RouteLocation.CoordinatePrecision,
            stopLocation.ComplexType.FindProperty(nameof(RouteLocation.Latitude))?.GetPrecision());
        Assert.Equal(
            RouteLocation.CoordinateScale,
            stopLocation.ComplexType.FindProperty(nameof(RouteLocation.Longitude))?.GetScale());

        var stopSequenceIndex = stopEntity.GetIndexes().Single(index =>
            index.Properties.Count == 2 &&
            index.Properties[0].Name == nameof(RouteStop.RouteId) &&
            index.Properties[1].Name == nameof(RouteStop.Sequence));
        Assert.True(stopSequenceIndex.IsUnique);

        var routeForeignKey = stopEntity.GetForeignKeys().Single(foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(Route));
        Assert.Equal(DeleteBehavior.Cascade, routeForeignKey.DeleteBehavior);
    }
}
