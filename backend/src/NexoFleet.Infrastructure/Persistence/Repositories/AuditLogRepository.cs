using Microsoft.EntityFrameworkCore;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Application.Auditing.Dtos;
using NexoFleet.Domain.Auditing;

namespace NexoFleet.Infrastructure.Persistence.Repositories;

internal sealed class AuditLogRepository(ApplicationDbContext dbContext) : IAuditLogRepository
{
    public Task<AuditLog?> GetByIdAsync(
        Guid companyId,
        Guid id,
        CancellationToken cancellationToken = default) =>
        dbContext.AuditLogs.SingleOrDefaultAsync(
            log => log.CompanyId == companyId && log.Id == id,
            cancellationToken);

    public async Task<IReadOnlyList<AuditLog>> ListByCompanyIdAsync(
        Guid? companyId,
        CancellationToken cancellationToken = default) =>
        await dbContext.AuditLogs
            .Where(log => !companyId.HasValue || log.CompanyId == companyId.Value)
            .OrderByDescending(log => log.OccurredAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AuditLog>> SearchAsync(
        Guid? companyId,
        string? search,
        string? entityType,
        string? severity,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.AuditLogs
            .Where(log => !companyId.HasValue || log.CompanyId == companyId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(log => 
                log.Action.ToLower().Contains(s) || 
                (log.ActorEmail != null && log.ActorEmail.ToLower().Contains(s)) ||
                (log.IpAddress != null && log.IpAddress.ToLower().Contains(s)) ||
                (log.Data != null && log.Data.ToLower().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(entityType) && !entityType.Equals("ALL", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(log => log.EntityType == entityType);
        }

        if (!string.IsNullOrWhiteSpace(severity) && !severity.Equals("ALL", StringComparison.OrdinalIgnoreCase) && Enum.TryParse<AuditLogSeverity>(severity, true, out var parsedSeverity))
        {
            query = query.Where(log => log.Severity == parsedSeverity);
        }

        return await query
            .OrderByDescending(log => log.OccurredAtUtc)
            .Take(100)
            .ToListAsync(cancellationToken);
    }

    public async Task<AuditLogStatsResponse> GetStatsAsync(
        Guid? companyId,
        CancellationToken cancellationToken = default)
    {
        var baseQuery = dbContext.AuditLogs
            .Where(log => !companyId.HasValue || log.CompanyId == companyId.Value);

        var total = await baseQuery.CountAsync(cancellationToken);
        var companyOps = await baseQuery.CountAsync(log => log.EntityType == "Company" || log.EntityType == "User" || log.EntityType == "Role", cancellationToken);
        var securityEvents = await baseQuery.CountAsync(log => log.EntityType == "Security", cancellationToken);
        var alerts = await baseQuery.CountAsync(log => log.Severity == AuditLogSeverity.Warning || log.Severity == AuditLogSeverity.Critical, cancellationToken);

        return new AuditLogStatsResponse(total, companyOps, securityEvents, alerts);
    }

    public void Add(AuditLog auditLog) => dbContext.AuditLogs.Add(auditLog);
}
