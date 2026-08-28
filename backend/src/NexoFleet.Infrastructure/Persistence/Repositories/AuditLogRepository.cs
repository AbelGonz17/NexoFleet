using Microsoft.EntityFrameworkCore;
using NexoFleet.Domain.Auditing;

namespace NexoFleet.Infrastructure.Persistence.Repositories;

internal sealed class AuditLogRepository(ApplicationDbContext dbContext) : IAuditLogRepository
{
    public Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.AuditLogs.SingleOrDefaultAsync(log => log.Id == id, cancellationToken);

    public void Add(AuditLog auditLog) => dbContext.AuditLogs.Add(auditLog);
}
