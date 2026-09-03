using Microsoft.EntityFrameworkCore;
using NexoFleet.Application.Abstractions.Persistence;
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

    public void Add(AuditLog auditLog) => dbContext.AuditLogs.Add(auditLog);
}
