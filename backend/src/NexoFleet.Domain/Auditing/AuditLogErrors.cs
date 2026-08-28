using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Auditing;

public static class AuditLogErrors
{
    public const int ActionMaxLength = 100;
    public const int EntityTypeMaxLength = 100;
    public const int DataMaxLength = 10000;
    public const int IpAddressMaxLength = 64;
    public const int UserAgentMaxLength = 500;

    public static readonly Error InvalidId = Error.Validation("AuditLog.InvalidId", "The audit log identifier is invalid.");
    public static readonly Error InvalidCompanyId = Error.Validation("AuditLog.InvalidCompanyId", "The company identifier is invalid.");
    public static readonly Error InvalidActorUserId = Error.Validation("AuditLog.InvalidActorUserId", "The actor user identifier is invalid.");
    public static readonly Error ActionRequired = Error.Validation("AuditLog.ActionRequired", "The audited action is required.");
    public static readonly Error ActionTooLong = Error.Validation("AuditLog.ActionTooLong", "The audited action is too long.");
    public static readonly Error EntityTypeRequired = Error.Validation("AuditLog.EntityTypeRequired", "The audited entity type is required.");
    public static readonly Error EntityTypeTooLong = Error.Validation("AuditLog.EntityTypeTooLong", "The audited entity type is too long.");
    public static readonly Error DataTooLong = Error.Validation("AuditLog.DataTooLong", "The audit data is too long.");
    public static readonly Error DataInvalid = Error.Validation("AuditLog.DataInvalid", "The audit data must be valid JSON.");
    public static readonly Error IpAddressTooLong = Error.Validation("AuditLog.IpAddressTooLong", "The IP address is too long.");
    public static readonly Error UserAgentTooLong = Error.Validation("AuditLog.UserAgentTooLong", "The user agent is too long.");
}
