using Microsoft.EntityFrameworkCore;
using NexoFleet.Domain.Companies;
using NexoFleet.Domain.Employees;
using NexoFleet.Domain.Vehicles;
using NexoFleet.Infrastructure.Persistence;

namespace NexoFleet.Infrastructure.UnitTests.Persistence;

public sealed class VehicleConfigurationTests
{
    [Fact]
    public void VehicleShouldHaveTheExpectedDatabaseConfiguration()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=nexofleet;Username=test;Password=test")
            .Options;
        using var context = new ApplicationDbContext(options);

        var entity = context.Model.FindEntityType(typeof(Vehicle));

        Assert.NotNull(entity);
        Assert.Equal("vehicles", entity.GetTableName());
        Assert.Equal(
            Vehicle.LicensePlateMaxLength,
            entity.FindProperty(nameof(Vehicle.LicensePlate))?.GetMaxLength());
        Assert.Equal(
            typeof(string),
            entity.FindProperty(nameof(Vehicle.OwnershipType))?.GetProviderClrType());
        Assert.Equal(
            typeof(string),
            entity.FindProperty(nameof(Vehicle.Type))?.GetProviderClrType());
        Assert.Equal(
            typeof(string),
            entity.FindProperty(nameof(Vehicle.Status))?.GetProviderClrType());
        Assert.Equal(
            typeof(string),
            entity.FindProperty(nameof(Vehicle.ApprovalStatus))?.GetProviderClrType());
        Assert.Equal(
            VehicleErrors.ApprovalReasonMaxLength,
            entity.FindProperty(nameof(Vehicle.ApprovalDecisionReason))?.GetMaxLength());

        var licensePlateIndex = entity.GetIndexes().Single(index =>
            index.Properties.Count == 2 &&
            index.Properties[0].Name == nameof(Vehicle.CompanyId) &&
            index.Properties[1].Name == nameof(Vehicle.LicensePlate));
        Assert.True(licensePlateIndex.IsUnique);

        var ownerIndex = entity.GetIndexes().Single(index =>
            index.Properties.Count == 1 &&
            index.Properties[0].Name == nameof(Vehicle.OwnerEmployeeId));
        Assert.False(ownerIndex.IsUnique);

        var availabilityIndex = entity.GetIndexes().Single(index =>
            index.Properties.Count == 3 &&
            index.Properties[0].Name == nameof(Vehicle.CompanyId) &&
            index.Properties[1].Name == nameof(Vehicle.ApprovalStatus) &&
            index.Properties[2].Name == nameof(Vehicle.Status));
        Assert.False(availabilityIndex.IsUnique);

        var companyForeignKey = entity.GetForeignKeys().Single(foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(Company));
        Assert.Equal(DeleteBehavior.Restrict, companyForeignKey.DeleteBehavior);

        var employeeForeignKey = entity.GetForeignKeys().Single(foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(Employee));
        Assert.Equal(DeleteBehavior.Restrict, employeeForeignKey.DeleteBehavior);
        Assert.False(employeeForeignKey.IsUnique);
        Assert.Collection(
            employeeForeignKey.Properties,
            property => Assert.Equal(nameof(Vehicle.CompanyId), property.Name),
            property => Assert.Equal(nameof(Vehicle.OwnerEmployeeId), property.Name));
        Assert.Collection(
            employeeForeignKey.PrincipalKey.Properties,
            property => Assert.Equal(nameof(Employee.CompanyId), property.Name),
            property => Assert.Equal(nameof(Employee.Id), property.Name));
    }
}
