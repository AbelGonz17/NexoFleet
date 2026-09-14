using Microsoft.EntityFrameworkCore;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Application.Dashboards.Dtos;
using NexoFleet.Domain.Companies;
using NexoFleet.Domain.Trips;

namespace NexoFleet.Infrastructure.Persistence.Repositories;

internal sealed class DashboardQueries(ApplicationDbContext dbContext) : IDashboardQueries
{
    public async Task<IReadOnlyList<GlobalKpiResponse>> GetGlobalKpisAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var startOfCurrentMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var startOfPreviousMonth = startOfCurrentMonth.AddMonths(-1);

        // Empresas
        var currentCompaniesCount = await dbContext.Companies.CountAsync(cancellationToken);
        var newCompaniesThisMonth = await dbContext.Companies.CountAsync(c => c.CreatedAtUtc >= startOfCurrentMonth, cancellationToken);
        var activeCompanies = await dbContext.Companies.CountAsync(c => c.Status == CompanyStatus.Active, cancellationToken);
        var suspendedCompanies = await dbContext.Companies.CountAsync(c => c.Status == CompanyStatus.Suspended, cancellationToken);

        // Flota
        var currentVehiclesCount = await dbContext.Vehicles.CountAsync(cancellationToken);
        var newVehiclesThisMonth = await dbContext.Vehicles.CountAsync(c => c.CreatedAtUtc >= startOfCurrentMonth, cancellationToken);

        // Usuarios
        var currentUsersCount = await dbContext.Users.CountAsync(cancellationToken);
        
        // Despachos
        var currentTripsCount = await dbContext.Trips.CountAsync(cancellationToken);
        var newTripsThisMonth = await dbContext.Trips.CountAsync(c => c.CreatedAtUtc >= startOfCurrentMonth, cancellationToken);

        return new List<GlobalKpiResponse>
        {
            new GlobalKpiResponse(
                "Empresas en la Red", 
                currentCompaniesCount.ToString(System.Globalization.CultureInfo.InvariantCulture), 
                $"{activeCompanies} Activas · {suspendedCompanies} Suspendidas", 
                $"+{newCompaniesThisMonth} este mes", 
                newCompaniesThisMonth > 0 ? "up" : "down", 
                "Building2"),
            new GlobalKpiResponse(
                "Flota Global", 
                currentVehiclesCount.ToString(System.Globalization.CultureInfo.InvariantCulture), 
                "Vehículos registrados", 
                $"+{newVehiclesThisMonth} este mes", 
                newVehiclesThisMonth > 0 ? "up" : "down", 
                "Truck"),
            new GlobalKpiResponse(
                "Usuarios", 
                currentUsersCount.ToString(System.Globalization.CultureInfo.InvariantCulture), 
                "Usuarios activos", 
                "N/A", 
                "up", 
                "Users"),
            new GlobalKpiResponse(
                "Despachos Totales", 
                currentTripsCount.ToString(System.Globalization.CultureInfo.InvariantCulture), 
                "Viajes históricos", 
                $"+{newTripsThisMonth} este mes", 
                newTripsThisMonth > 0 ? "up" : "down", 
                "Map")
        };
    }

    public async Task<IReadOnlyList<TopCompanyResponse>> GetTopCompaniesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var startOfCurrentMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);

        var topCompanies = await dbContext.Companies
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.TaxIdentification,
                c.Status,
                FleetCount = dbContext.Vehicles.Count(v => v.CompanyId == c.Id),
                TripsThisMonth = dbContext.Trips.Count(t => t.CompanyId == c.Id && t.CreatedAtUtc >= startOfCurrentMonth)
            })
            .OrderByDescending(c => c.TripsThisMonth)
            .Take(10)
            .ToListAsync(cancellationToken);

        return topCompanies.Select(c => new TopCompanyResponse(
            c.Id.ToString(),
            c.Name.Value,
            c.TaxIdentification,
            c.FleetCount,
            c.TripsThisMonth,
            c.Status.ToString(),
            "admin@empresa.com" // Placeholder since admin mapping requires identity join
        )).ToList();
    }

    public async Task<IReadOnlyList<AuditFeedResponse>> GetAuditFeedAsync(CancellationToken cancellationToken = default)
    {
        var logs = await dbContext.AuditLogs
            .OrderByDescending(l => l.OccurredAtUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        return logs.Select(l => new AuditFeedResponse(
            l.Id.ToString(),
            l.Action,
            $"{l.Action} en {l.EntityType}",
            l.ActorUserId.ToString(),
            l.OccurredAtUtc.ToString("g", System.Globalization.CultureInfo.InvariantCulture),
            DetermineSeverity(l.Action)
        )).ToList();
    }

    public async Task<CompanyStatsResponse> GetCompanyStatsAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var startOfToday = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
        
        var activeTrips = await dbContext.Trips
            .CountAsync(t => t.CompanyId == companyId && (t.Status == TripStatus.InProgress || t.Status == TripStatus.Assigned), cancellationToken);

        var kpis = new List<CompanyKpiResponse>
        {
            new CompanyKpiResponse("Viajes Activos", activeTrips.ToString(System.Globalization.CultureInfo.InvariantCulture), "", "up", "Navigation")
        };

        var enRuta = await dbContext.Vehicles.CountAsync(v => v.CompanyId == companyId, cancellationToken); // Placeholder
        var fleetStatus = new FleetStatusResponse(enRuta, 0, 0);

        return new CompanyStatsResponse(kpis, fleetStatus);
    }

    public async Task<IReadOnlyList<RecentTripResponse>> GetRecentTripsAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var trips = await dbContext.Trips
            .Where(t => t.CompanyId == companyId)
            .OrderByDescending(t => t.CreatedAtUtc)
            .Take(10)
            .ToListAsync(cancellationToken);

        return trips.Select(t => new RecentTripResponse(
            t.TripNumber,
            t.RouteId?.ToString() ?? "Sin ruta", // Route Name placeholder
            t.CurrentAssignment?.EmployeeId.ToString() ?? "Sin conductor", // Driver Name placeholder
            t.CurrentAssignment?.VehicleId?.ToString() ?? "Sin vehículo",
            t.Status.ToString(),
            t.CreatedAtUtc.ToString("g", System.Globalization.CultureInfo.InvariantCulture)
        )).ToList();
    }

    private static string DetermineSeverity(string action)
    {
        if (action.Contains("SUSPEND") || action.Contains("DELAY")) return "warning";
        if (action.Contains("DELETE") || action.Contains("ERROR") || action.Contains("LOCK") || action.Contains("DENY")) return "error";
        if (action.Contains("CREATE") || action.Contains("UPDATE") || action.Contains("LOGIN") || action.Contains("SUCCESS")) return "info";
        return "info"; // default
    }
}
