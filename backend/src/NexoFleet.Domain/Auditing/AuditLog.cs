using NexoFleet.Domain.Common;
using System.Text.Json;

namespace NexoFleet.Domain.Auditing;

public sealed class AuditLog : AggregateRoot
{
    private AuditLog(
        Guid id,
        Guid? companyId,
        Guid actorUserId,
        string action,
        string entityType,
        Guid? entityId,
        string? data,
        string? ipAddress,
        string? userAgent,
        DateTimeOffset occurredAtUtc) : base(id)
    {
        CompanyId = companyId;
        ActorUserId = actorUserId;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        Data = data;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        OccurredAtUtc = occurredAtUtc;
    }

    private AuditLog() { }

    public Guid? CompanyId { get; private set; }
    public Guid ActorUserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public Guid? EntityId { get; private set; }
    public string? Data { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }

    public static Result<AuditLog> Create(
        Guid id,
        Guid? companyId,
        Guid actorUserId,
        string action,
        string entityType,
        Guid? entityId,
        string? data,
        string? ipAddress,
        string? userAgent,
        DateTimeOffset occurredAtUtc)
    {
        if (id == Guid.Empty) return Result<AuditLog>.Failure(AuditLogErrors.InvalidId);
        if (companyId == Guid.Empty) return Result<AuditLog>.Failure(AuditLogErrors.InvalidCompanyId);
        if (actorUserId == Guid.Empty) return Result<AuditLog>.Failure(AuditLogErrors.InvalidActorUserId);
        if (string.IsNullOrWhiteSpace(action)) return Result<AuditLog>.Failure(AuditLogErrors.ActionRequired);
        if (action.Trim().Length > AuditLogErrors.ActionMaxLength) return Result<AuditLog>.Failure(AuditLogErrors.ActionTooLong);
        if (string.IsNullOrWhiteSpace(entityType)) return Result<AuditLog>.Failure(AuditLogErrors.EntityTypeRequired);
        if (entityType.Trim().Length > AuditLogErrors.EntityTypeMaxLength) return Result<AuditLog>.Failure(AuditLogErrors.EntityTypeTooLong);
        if (data?.Trim().Length > AuditLogErrors.DataMaxLength) return Result<AuditLog>.Failure(AuditLogErrors.DataTooLong);
        if (!string.IsNullOrWhiteSpace(data) && !IsValidJson(data)) return Result<AuditLog>.Failure(AuditLogErrors.DataInvalid);
        if (ipAddress?.Trim().Length > AuditLogErrors.IpAddressMaxLength) return Result<AuditLog>.Failure(AuditLogErrors.IpAddressTooLong);
        if (userAgent?.Trim().Length > AuditLogErrors.UserAgentMaxLength) return Result<AuditLog>.Failure(AuditLogErrors.UserAgentTooLong);

        return Result<AuditLog>.Success(new AuditLog(
            id, companyId, actorUserId, Normalize(action), Normalize(entityType), entityId,
            NormalizeOptional(data), NormalizeOptional(ipAddress), NormalizeOptional(userAgent), occurredAtUtc));
    }

    private static string Normalize(string value) => value.Trim();
    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool IsValidJson(string value)
    {
        try
        {
            using var document = JsonDocument.Parse(value);
            return document.RootElement.ValueKind is JsonValueKind.Object or JsonValueKind.Array;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
