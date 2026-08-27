using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using NexoFleet.Domain.Companies;
using NexoFleet.Domain.Employees;
using NexoFleet.Infrastructure.Identity;
using NexoFleet.Infrastructure.Persistence;

namespace NexoFleet.Infrastructure.UnitTests.Persistence;

public sealed class EmployeeConfigurationTests
{
    [Fact]
    public void EmployeeShouldHaveTheExpectedDatabaseConfiguration()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=nexofleet;Username=test;Password=test")
            .Options;
        using var context = new ApplicationDbContext(options);

        var entity = context.Model.FindEntityType(typeof(Employee));

        Assert.NotNull(entity);
        Assert.Equal("employees", entity.GetTableName());
        Assert.Equal(
            Employee.EmployeeCodeMaxLength,
            entity.FindProperty(nameof(Employee.EmployeeCode))?.GetMaxLength());
        Assert.Equal(
            Employee.EmailMaxLength,
            entity.FindProperty(nameof(Employee.Email))?.GetMaxLength());
        Assert.Equal(
            typeof(string),
            entity.FindProperty(nameof(Employee.Status))?.GetProviderClrType());

        AssertUniqueCompositeIndex(entity, nameof(Employee.CompanyId), nameof(Employee.EmployeeCode));
        AssertUniqueCompositeIndex(entity, nameof(Employee.CompanyId), nameof(Employee.IdentityDocument));
        AssertUniqueCompositeIndex(entity, nameof(Employee.CompanyId), nameof(Employee.Email));

        var userIndex = entity.GetIndexes().Single(index =>
            index.Properties.Count == 1 &&
            index.Properties[0].Name == nameof(Employee.UserId));
        Assert.True(userIndex.IsUnique);

        var companyForeignKey = entity.GetForeignKeys().Single(foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(Company));
        Assert.Equal(DeleteBehavior.Restrict, companyForeignKey.DeleteBehavior);

        var userForeignKey = entity.GetForeignKeys().Single(foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(ApplicationUser));
        Assert.Equal(DeleteBehavior.Restrict, userForeignKey.DeleteBehavior);
        Assert.True(userForeignKey.IsUnique);
    }

    private static void AssertUniqueCompositeIndex(
        IEntityType entity,
        string firstProperty,
        string secondProperty)
    {
        var index = entity.GetIndexes().Single(candidate =>
            candidate.Properties.Count == 2 &&
            candidate.Properties[0].Name == firstProperty &&
            candidate.Properties[1].Name == secondProperty);

        Assert.True(index.IsUnique);
    }
}
