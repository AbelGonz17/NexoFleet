using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NexoFleet.Domain.Auditing;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Domain.Clients;
using NexoFleet.Domain.Companies;
using NexoFleet.Domain.Employees;
using NexoFleet.Domain.Notifications;
using NexoFleet.Domain.Payments;
using NexoFleet.Domain.Routes;
using NexoFleet.Domain.RouteSchedules;
using NexoFleet.Domain.Trips;
using NexoFleet.Domain.Vehicles;
using NexoFleet.Infrastructure.Identity;

namespace NexoFleet.Infrastructure.Persistence;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options),
      IApplicationDbContext
{
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<Company> Companies => Set<Company>();

    public DbSet<Client> Clients => Set<Client>();

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    public DbSet<Route> Routes => Set<Route>();

    public DbSet<RouteSchedule> RouteSchedules => Set<RouteSchedule>();

    public DbSet<Trip> Trips => Set<Trip>();

    public DbSet<PaymentPeriod> PaymentPeriods => Set<PaymentPeriod>();

    public DbSet<PaymentReport> PaymentReports => Set<PaymentReport>();

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        builder.UseSnakeCaseNames();
    }
}
