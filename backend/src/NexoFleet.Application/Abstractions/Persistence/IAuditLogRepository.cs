using NexoFleet.Domain.Auditing;

namespace NexoFleet.Application.Abstractions.Persistence;

public interface IAuditLogRepository
{
    Task<AuditLog?> GetByIdAsync(
        Guid companyId,
        Guid id,
        CancellationToken cancellationToken = default);

    void Add(AuditLog auditLog);
}
