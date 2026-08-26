using Microsoft.Extensions.DependencyInjection;

namespace NexoFleet.Application.UnitTests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddApplicationShouldReturnServiceCollection()
    {
        var services = new ServiceCollection();

        var result = NexoFleet.Application.DependencyInjection.AddApplication(services);

        Assert.Same(services, result);
    }
}
