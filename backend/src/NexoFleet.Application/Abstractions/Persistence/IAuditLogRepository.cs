using NexoFleet.Domain.Auditing;
using NexoFleet.Application.Auditing.Dtos;

namespace NexoFleet.Application.Abstractions.Persistence;

public interface IAuditLogRepository
{
    Task<AuditLog?> GetByIdAsync(
        Guid companyId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLog>> ListByCompanyIdAsync(
        Guid? companyId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLog>> SearchAsync(
        Guid? companyId,
        string? search,
        string? entityType,
        string? severity,
        CancellationToken cancellationToken = default);

    Task<AuditLogStatsResponse> GetStatsAsync(
        Guid? companyId,
        CancellationToken cancellationToken = default);

    void Add(AuditLog auditLog);
}
