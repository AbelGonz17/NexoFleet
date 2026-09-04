using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace NexoFleet.Api.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SkipDatabaseInitialization"] = "true",
                ["ConnectionStrings:Database"] = "Host=localhost;Port=5432;Database=nexofleet_test;Username=postgres;Password=postgres"
            });
        });
    }
}
