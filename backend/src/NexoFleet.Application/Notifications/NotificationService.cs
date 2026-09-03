using FluentValidation;
using NexoFleet.Application.Abstractions.Context;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Application.Abstractions.Time;
using NexoFleet.Application.Common;
using NexoFleet.Application.Notifications.Dtos;
using NexoFleet.Domain.Common;
using NexoFleet.Domain.Notifications;

namespace NexoFleet.Application.Notifications;

public sealed class NotificationService(
    INotificationRepository notificationRepository,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<CreateNotificationRequest> createValidator)
{
    public async Task<Result<NotificationResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<NotificationResponse>.Failure(NotificationErrors.InvalidCompanyId);
        }

        var notification = await notificationRepository.GetByIdAsync(companyId, id, cancellationToken);
        return notification is null
            ? Result<NotificationResponse>.Failure(NotificationErrors.NotFound)
            : Result<NotificationResponse>.Success(NotificationResponse.FromDomain(notification));
    }

    public async Task<Result<IReadOnlyList<NotificationResponse>>> GetMyNotificationsAsync(
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<IReadOnlyList<NotificationResponse>>.Failure(NotificationErrors.InvalidCompanyId);
        }

        if (currentUser.UserId is not { } recipientUserId)
        {
            return Result<IReadOnlyList<NotificationResponse>>.Failure(NotificationErrors.InvalidUserId);
        }

        var notifications = await notificationRepository.GetByRecipientAsync(companyId, recipientUserId, cancellationToken);
        var responses = notifications.Select(NotificationResponse.FromDomain).ToArray();
        return Result<IReadOnlyList<NotificationResponse>>.Success(responses);
    }

    public async Task<Result<IReadOnlyList<NotificationResponse>>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<IReadOnlyList<NotificationResponse>>.Failure(NotificationErrors.InvalidCompanyId);
        }

        var notifications = await notificationRepository.ListByCompanyIdAsync(companyId, cancellationToken);
        var responses = notifications.Select(NotificationResponse.FromDomain).ToArray();
        return Result<IReadOnlyList<NotificationResponse>>.Success(responses);
    }

    public async Task<Result<NotificationResponse>> CreateAsync(
        CreateNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<NotificationResponse>.Failure(NotificationErrors.InvalidCompanyId);
        }

        var validationResult = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<NotificationResponse>.Failure(validationResult.ToValidationError());
        }

        var notificationResult = Notification.Create(
            Guid.NewGuid(),
            companyId,
            request.RecipientUserId,
            request.RecipientEmployeeId,
            request.Type,
            request.Title,
            request.Message,
            request.RelatedEntityType,
            request.RelatedEntityId,
            clock.UtcNow);

        if (notificationResult.IsFailure)
        {
            return Result<NotificationResponse>.Failure(notificationResult.Error);
        }

        notificationRepository.Add(notificationResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<NotificationResponse>.Success(NotificationResponse.FromDomain(notificationResult.Value));
    }

    public async Task<Result> MarkAsReadAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result.Failure(NotificationErrors.InvalidCompanyId);
        }

        var notification = await notificationRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (notification is null)
        {
            return Result.Failure(NotificationErrors.NotFound);
        }

        var markResult = notification.MarkAsRead(clock.UtcNow);
        if (markResult.IsFailure)
        {
            return markResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> ArchiveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result.Failure(NotificationErrors.InvalidCompanyId);
        }

        var notification = await notificationRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (notification is null)
        {
            return Result.Failure(NotificationErrors.NotFound);
        }

        var archiveResult = notification.Archive(clock.UtcNow);
        if (archiveResult.IsFailure)
        {
            return archiveResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
