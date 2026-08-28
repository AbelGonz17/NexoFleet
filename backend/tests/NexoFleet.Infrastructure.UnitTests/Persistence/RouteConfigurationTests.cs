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
