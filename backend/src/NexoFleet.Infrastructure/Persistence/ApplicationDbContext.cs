using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Domain.Companies;
using NexoFleet.Domain.Employees;
using NexoFleet.Domain.Routes;
using NexoFleet.Domain.RouteSchedules;
using NexoFleet.Domain.Vehicles;
using NexoFleet.Infrastructure.Identity;

namespace NexoFleet.Infrastructure.Persistence;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options),
      IApplicationDbContext
{
    public DbSet<Company> Companies => Set<Company>();

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    public DbSet<Route> Routes => Set<Route>();

    public DbSet<RouteSchedule> RouteSchedules => Set<RouteSchedule>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
