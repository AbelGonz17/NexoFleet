using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using NexoFleet.Domain.Companies;
using NexoFleet.Infrastructure.Persistence;
using NexoFleet.Infrastructure.Identity;

namespace NexoFleet.Infrastructure.UnitTests.Persistence;

public sealed class CompanyConfigurationTests
{
    [Fact]
    public void CompanyShouldHaveTheExpectedDatabaseConfiguration()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=nexofleet;Username=test;Password=test")
            .Options;
        using var context = new ApplicationDbContext(options);

        var entity = context.Model.FindEntityType(typeof(Company));

        Assert.NotNull(entity);
        Assert.Equal("companies", entity.GetTableName());
        Assert.Equal(
            Company.NameMaxLength,
            entity.FindProperty(nameof(Company.Name))?.GetMaxLength());
        Assert.Equal(
            Company.EmailMaxLength,
            entity.FindProperty(nameof(Company.Email))?.GetMaxLength());
        Assert.Equal(
            typeof(string),
            entity.FindProperty(nameof(Company.Status))?.GetProviderClrType());

        var taxIndex = entity.GetIndexes().Single(index =>
            index.Properties.Single().Name == nameof(Company.TaxIdentification));
        Assert.True(taxIndex.IsUnique);

        var userEntity = context.Model.FindEntityType(typeof(ApplicationUser));
        var companyForeignKey = userEntity?.GetForeignKeys().Single(foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(Company));

        Assert.NotNull(companyForeignKey);
        Assert.Equal(DeleteBehavior.Restrict, companyForeignKey.DeleteBehavior);
    }
}
