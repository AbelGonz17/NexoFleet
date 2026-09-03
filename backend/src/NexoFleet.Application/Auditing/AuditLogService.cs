using FluentValidation;
using NexoFleet.Application.Abstractions.Context;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Application.Abstractions.Time;
using NexoFleet.Application.Auditing.Dtos;
using NexoFleet.Application.Common;
using NexoFleet.Domain.Auditing;
using NexoFleet.Domain.Common;

namespace NexoFleet.Application.Auditing;

public sealed class AuditLogService(
    IAuditLogRepository auditLogRepository,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<CreateAuditLogRequest> createValidator)
{
    public async Task<Result<AuditLogResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var companyId = currentTenant.CompanyId;
        if (!companyId.HasValue)
        {
            return Result<AuditLogResponse>.Failure(AuditLogErrors.InvalidCompanyId);
        }

        var log = await auditLogRepository.GetByIdAsync(companyId.Value, id, cancellationToken);
        return log is null
            ? Result<AuditLogResponse>.Failure(AuditLogErrors.NotFound)
            : Result<AuditLogResponse>.Success(AuditLogResponse.FromDomain(log));
    }

    public async Task<Result<IReadOnlyList<AuditLogResponse>>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var logs = await auditLogRepository.ListByCompanyIdAsync(currentTenant.CompanyId, cancellationToken);
        var responses = logs.Select(AuditLogResponse.FromDomain).ToArray();
        return Result<IReadOnlyList<AuditLogResponse>>.Success(responses);
    }

    public async Task<Result<AuditLogResponse>> LogAsync(
        CreateAuditLogRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is not { } actorUserId)
        {
            return Result<AuditLogResponse>.Failure(AuditLogErrors.InvalidActorUserId);
        }

        var validationResult = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<AuditLogResponse>.Failure(validationResult.ToValidationError());
        }

        var auditLogResult = AuditLog.Create(
            Guid.NewGuid(),
            currentTenant.CompanyId,
            actorUserId,
            request.Action,
            request.EntityType,
            request.EntityId,
            request.Data,
            request.IpAddress,
            request.UserAgent,
            clock.UtcNow);

        if (auditLogResult.IsFailure)
        {
            return Result<AuditLogResponse>.Failure(auditLogResult.Error);
        }

        auditLogRepository.Add(auditLogResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AuditLogResponse>.Success(AuditLogResponse.FromDomain(auditLogResult.Value));
    }
}
