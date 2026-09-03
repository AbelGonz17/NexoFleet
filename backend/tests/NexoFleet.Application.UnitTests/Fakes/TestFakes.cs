using NexoFleet.Application.Abstractions.Context;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Application.Abstractions.Time;
using NexoFleet.Domain.Auditing;
using NexoFleet.Domain.Clients;
using NexoFleet.Domain.Companies;
using NexoFleet.Domain.Employees;
using NexoFleet.Domain.Notifications;
using NexoFleet.Domain.Payments;
using NexoFleet.Domain.Routes;
using NexoFleet.Domain.RouteSchedules;
using NexoFleet.Domain.Trips;
using NexoFleet.Domain.Vehicles;

namespace NexoFleet.Application.UnitTests.Fakes;

public sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCalls { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCalls++;
        return Task.FromResult(1);
    }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
    {
        await operation(cancellationToken);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken = default)
    {
        var result = await operation(cancellationToken);
        await SaveChangesAsync(cancellationToken);
        return result;
    }
}

public sealed class FakeClock(DateTimeOffset utcNow) : IClock
{
    public DateTimeOffset UtcNow { get; set; } = utcNow;
}

public sealed class FakeCurrentTenant(Guid? companyId) : ICurrentTenant
{
    public Guid? CompanyId { get; set; } = companyId;
    public bool IsAvailable => CompanyId.HasValue;
}

public sealed class FakeCurrentUser(Guid? userId, string? role = null) : ICurrentUser
{
    public Guid? UserId { get; set; } = userId;
    public string? Role { get; set; } = role;
    public bool IsAuthenticated => UserId.HasValue;
}

public sealed class FakeCompanyRepository : ICompanyRepository
{
    public List<Company> Companies { get; } = [];

    public Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Companies.SingleOrDefault(c => c.Id == id));

    public Task<bool> ExistsByTaxIdentificationAsync(string taxIdentification, Guid? excludingCompanyId = null, CancellationToken cancellationToken = default)
    {
        var normalized = taxIdentification.Trim().ToUpperInvariant();
        return Task.FromResult(Companies.Any(c =>
            c.TaxIdentification.Value == normalized &&
            (!excludingCompanyId.HasValue || c.Id != excludingCompanyId.Value)));
    }

    public Task<IReadOnlyList<Company>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Company>>(Companies.OrderBy(c => c.Name.Value).ToArray());

    public void Add(Company company) => Companies.Add(company);
}

public sealed class FakeClientRepository : IClientRepository
{
    public List<Client> Clients { get; } = [];

    public Task<Client?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Clients.SingleOrDefault(c => c.CompanyId == companyId && c.Id == id));

    public Task<bool> ExistsByCodeAsync(Guid companyId, string clientCode, Guid? excludingClientId = null, CancellationToken cancellationToken = default)
    {
        var normalized = clientCode.Trim().ToUpperInvariant();
        return Task.FromResult(Clients.Any(c =>
            c.CompanyId == companyId &&
            c.ClientCode.Value == normalized &&
            (!excludingClientId.HasValue || c.Id != excludingClientId.Value)));
    }

    public Task<IReadOnlyList<Client>> ListByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Client>>(Clients.Where(c => c.CompanyId == companyId).OrderBy(c => c.Name.Value).ToArray());

    public void Add(Client client) => Clients.Add(client);
}

public sealed class FakeEmployeeRepository : IEmployeeRepository
{
    public List<Employee> Employees { get; } = [];

    public Task<Employee?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Employees.SingleOrDefault(e => e.CompanyId == companyId && e.Id == id));

    public Task<Employee?> GetByUserIdAsync(Guid companyId, Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Employees.SingleOrDefault(e => e.CompanyId == companyId && e.UserId == userId));

    public Task<bool> ExistsByEmployeeCodeAsync(Guid companyId, string employeeCode, Guid? excludingEmployeeId = null, CancellationToken cancellationToken = default)
    {
        var normalized = employeeCode.Trim().ToUpperInvariant();
        return Task.FromResult(Employees.Any(e =>
            e.CompanyId == companyId &&
            e.EmployeeCode.Value == normalized &&
            (!excludingEmployeeId.HasValue || e.Id != excludingEmployeeId.Value)));
    }

    public Task<bool> ExistsByIdentityDocumentAsync(Guid companyId, string identityDocument, Guid? excludingEmployeeId = null, CancellationToken cancellationToken = default)
    {
        var normalized = identityDocument.Trim().ToUpperInvariant();
        return Task.FromResult(Employees.Any(e =>
            e.CompanyId == companyId &&
            e.IdentityDocument.Value == normalized &&
            (!excludingEmployeeId.HasValue || e.Id != excludingEmployeeId.Value)));
    }

    public Task<bool> ExistsByEmailAsync(Guid companyId, string email, Guid? excludingEmployeeId = null, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return Task.FromResult(Employees.Any(e =>
            e.CompanyId == companyId &&
            e.Email.Value == normalized &&
            (!excludingEmployeeId.HasValue || e.Id != excludingEmployeeId.Value)));
    }

    public Task<IReadOnlyList<Employee>> ListByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Employee>>(Employees.Where(e => e.CompanyId == companyId).OrderBy(e => e.FullName.ToString()).ToArray());

    public void Add(Employee employee) => Employees.Add(employee);
}

public sealed class FakeVehicleRepository : IVehicleRepository
{
    public List<Vehicle> Vehicles { get; } = [];

    public Task<Vehicle?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Vehicles.SingleOrDefault(v => v.CompanyId == companyId && v.Id == id));

    public Task<IReadOnlyList<Vehicle>> GetByOwnerEmployeeIdAsync(Guid companyId, Guid ownerEmployeeId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Vehicle>>(Vehicles.Where(v => v.CompanyId == companyId && v.OwnerEmployeeId == ownerEmployeeId).ToArray());

    public Task<bool> ExistsByLicensePlateAsync(Guid companyId, string licensePlate, Guid? excludingVehicleId = null, CancellationToken cancellationToken = default)
    {
        var normalized = licensePlate.Trim().ToUpperInvariant();
        return Task.FromResult(Vehicles.Any(v =>
            v.CompanyId == companyId &&
            v.LicensePlate == normalized &&
            (!excludingVehicleId.HasValue || v.Id != excludingVehicleId.Value)));
    }

    public Task<IReadOnlyList<Vehicle>> ListByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Vehicle>>(Vehicles.Where(v => v.CompanyId == companyId).OrderBy(v => v.LicensePlate).ToArray());

    public void Add(Vehicle vehicle) => Vehicles.Add(vehicle);
}

public sealed class FakeRouteRepository : IRouteRepository
{
    public List<Route> Routes { get; } = [];

    public Task<Route?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Routes.SingleOrDefault(r => r.CompanyId == companyId && r.Id == id));

    public Task<bool> ExistsByRouteCodeAsync(Guid companyId, string routeCode, Guid? excludingRouteId = null, CancellationToken cancellationToken = default)
    {
        var normalized = routeCode.Trim().ToUpperInvariant();
        var exists = Routes.Any(r => r.CompanyId == companyId && r.RouteCode == normalized && (!excludingRouteId.HasValue || r.Id != excludingRouteId.Value));
        return Task.FromResult(exists);
    }

    public Task<IReadOnlyList<Route>> ListByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Route>>(Routes.Where(r => r.CompanyId == companyId).OrderBy(r => r.Name).ToList());

    public void Add(Route route) => Routes.Add(route);
}

public sealed class FakeRouteScheduleRepository : IRouteScheduleRepository
{
    public List<RouteSchedule> Schedules { get; } = [];

    public Task<RouteSchedule?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Schedules.SingleOrDefault(s => s.CompanyId == companyId && s.Id == id));

    public Task<IReadOnlyList<RouteSchedule>> GetByRouteIdAsync(Guid companyId, Guid routeId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<RouteSchedule>>(Schedules.Where(s => s.CompanyId == companyId && s.RouteId == routeId).ToList());

    public Task<IReadOnlyList<RouteSchedule>> ListByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<RouteSchedule>>(Schedules.Where(s => s.CompanyId == companyId).OrderBy(s => s.StartTime).ToList());

    public void Add(RouteSchedule routeSchedule) => Schedules.Add(routeSchedule);
}

public sealed class FakeTripRepository : ITripRepository
{
    public List<Trip> Trips { get; } = [];

    public Task<Trip?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Trips.SingleOrDefault(t => t.CompanyId == companyId && t.Id == id));

    public Task<bool> ExistsByNumberAsync(Guid companyId, string tripNumber, CancellationToken cancellationToken = default)
    {
        var normalized = tripNumber.Trim().ToUpperInvariant();
        return Task.FromResult(Trips.Any(t => t.CompanyId == companyId && t.TripNumber == normalized));
    }

    public Task<bool> HasInProgressTripForVehicleAsync(Guid companyId, Guid vehicleId, Guid? excludingTripId = null, CancellationToken cancellationToken = default)
    {
        var exists = Trips.Any(t =>
            t.CompanyId == companyId &&
            t.Status == TripStatus.InProgress &&
            t.CurrentAssignment?.VehicleId == vehicleId &&
            (!excludingTripId.HasValue || t.Id != excludingTripId.Value));
        return Task.FromResult(exists);
    }

    public Task<IReadOnlyList<Trip>> ListByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Trip>>(Trips.Where(t => t.CompanyId == companyId).OrderByDescending(t => t.ServiceDate).ToList());

    public void Add(Trip trip) => Trips.Add(trip);
}

public sealed class FakePaymentPeriodRepository : IPaymentPeriodRepository
{
    public List<PaymentPeriod> Periods { get; } = [];

    public Task<PaymentPeriod?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Periods.SingleOrDefault(p => p.CompanyId == companyId && p.Id == id));

    public Task<bool> ExistsByCodeAsync(Guid companyId, string code, Guid? excludingPeriodId = null, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return Task.FromResult(Periods.Any(p =>
            p.CompanyId == companyId &&
            p.Code == normalized &&
            (!excludingPeriodId.HasValue || p.Id != excludingPeriodId.Value)));
    }

    public Task<IReadOnlyList<PaymentPeriod>> ListByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<PaymentPeriod>>(Periods.Where(p => p.CompanyId == companyId).OrderByDescending(p => p.StartsOn).ToList());

    public void Add(PaymentPeriod period) => Periods.Add(period);
}

public sealed class FakePaymentReportRepository : IPaymentReportRepository
{
    public List<PaymentReport> Reports { get; } = [];

    public Task<PaymentReport?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Reports.SingleOrDefault(r => r.CompanyId == companyId && r.Id == id));

    public Task<PaymentReport?> GetByPeriodAndEmployeeAsync(Guid companyId, Guid periodId, Guid employeeId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Reports.SingleOrDefault(r => r.CompanyId == companyId && r.PaymentPeriodId == periodId && r.EmployeeId == employeeId));

    public Task<IReadOnlyList<PaymentReport>> ListByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<PaymentReport>>(Reports.Where(r => r.CompanyId == companyId).OrderByDescending(r => r.CreatedAtUtc).ToList());

    public Task<IReadOnlyList<PaymentReport>> ListByPeriodIdAsync(Guid companyId, Guid periodId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<PaymentReport>>(Reports.Where(r => r.CompanyId == companyId && r.PaymentPeriodId == periodId).OrderByDescending(r => r.CreatedAtUtc).ToList());

    public void Add(PaymentReport report) => Reports.Add(report);
}

public sealed class FakeNotificationRepository : INotificationRepository
{
    public List<Notification> Notifications { get; } = [];

    public Task<Notification?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Notifications.SingleOrDefault(n => n.CompanyId == companyId && n.Id == id));

    public Task<IReadOnlyList<Notification>> GetByRecipientAsync(Guid companyId, Guid recipientUserId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Notification>>(Notifications.Where(n => n.CompanyId == companyId && n.RecipientUserId == recipientUserId).OrderByDescending(n => n.CreatedAtUtc).ToList());

    public Task<IReadOnlyList<Notification>> ListByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Notification>>(Notifications.Where(n => n.CompanyId == companyId).OrderByDescending(n => n.CreatedAtUtc).ToList());

    public void Add(Notification notification) => Notifications.Add(notification);
}

public sealed class FakeAuditLogRepository : IAuditLogRepository
{
    public List<AuditLog> Logs { get; } = [];

    public Task<AuditLog?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Logs.SingleOrDefault(l => l.CompanyId == companyId && l.Id == id));

    public Task<IReadOnlyList<AuditLog>> ListByCompanyIdAsync(Guid? companyId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<AuditLog>>(Logs.Where(l => !companyId.HasValue || l.CompanyId == companyId.Value).OrderByDescending(l => l.OccurredAtUtc).ToList());

    public void Add(AuditLog auditLog) => Logs.Add(auditLog);
}
