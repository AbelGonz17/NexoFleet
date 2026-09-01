using Microsoft.EntityFrameworkCore;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Infrastructure.Persistence;

namespace NexoFleet.Infrastructure.UnitTests.Persistence;

public sealed class UnitOfWorkTests
{
    [Fact]
    public void UnitOfWorkShouldImplementIUnitOfWork()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=nexofleet;Username=test;Password=test")
            .Options;
        using var context = new ApplicationDbContext(options);

        var unitOfWork = new UnitOfWork(context);

        Assert.IsAssignableFrom<IUnitOfWork>(unitOfWork);
    }
}
