using NexoFleet.Domain.Auditing;

namespace NexoFleet.Application.Auditing.Dtos;

public sealed record AuditLogResponse(
    Guid Id,
    Guid? CompanyId,
    Guid ActorUserId,
    string Action,
    string EntityType,
    Guid? EntityId,
    string? Data,
    string? IpAddress,
    string? UserAgent,
    DateTimeOffset OccurredAtUtc)
{
    public static AuditLogResponse FromDomain(AuditLog log) =>
        new(
            log.Id,
            log.CompanyId,
            log.ActorUserId,
            log.Action,
            log.EntityType,
            log.EntityId,
            log.Data,
            log.IpAddress,
            log.UserAgent,
            log.OccurredAtUtc);
}

public sealed record CreateAuditLogRequest(
    string Action,
    string EntityType,
    Guid? EntityId = null,
    string? Data = null,
    string? IpAddress = null,
    string? UserAgent = null);
