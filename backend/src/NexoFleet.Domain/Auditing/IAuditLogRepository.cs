namespace NexoFleet.Domain.Auditing;

public interface IAuditLogRepository
{
    Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(AuditLog auditLog);
}
