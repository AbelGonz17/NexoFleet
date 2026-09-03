using FluentValidation;
using NexoFleet.Application.Abstractions.Context;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Application.Abstractions.Time;
using NexoFleet.Application.Common;
using NexoFleet.Application.Payments.Dtos;
using NexoFleet.Domain.Common;
using NexoFleet.Domain.Payments;

namespace NexoFleet.Application.Payments;

public sealed class PaymentPeriodService(
    IPaymentPeriodRepository periodRepository,
    ICurrentTenant currentTenant,
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<CreatePaymentPeriodRequest> createValidator)
{
    public async Task<Result<PaymentPeriodResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<PaymentPeriodResponse>.Failure(PaymentErrors.InvalidCompanyId);
        }

        var period = await periodRepository.GetByIdAsync(companyId, id, cancellationToken);
        return period is null
            ? Result<PaymentPeriodResponse>.Failure(PaymentErrors.PeriodNotFound)
            : Result<PaymentPeriodResponse>.Success(PaymentPeriodResponse.FromDomain(period));
    }

    public async Task<Result<IReadOnlyList<PaymentPeriodResponse>>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<IReadOnlyList<PaymentPeriodResponse>>.Failure(PaymentErrors.InvalidCompanyId);
        }

        var periods = await periodRepository.ListByCompanyIdAsync(companyId, cancellationToken);
        var responses = periods.Select(PaymentPeriodResponse.FromDomain).ToArray();
        return Result<IReadOnlyList<PaymentPeriodResponse>>.Success(responses);
    }

    public async Task<Result<PaymentPeriodResponse>> CreateAsync(
        CreatePaymentPeriodRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<PaymentPeriodResponse>.Failure(PaymentErrors.InvalidCompanyId);
        }

        var validationResult = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<PaymentPeriodResponse>.Failure(validationResult.ToValidationError());
        }

        if (await periodRepository.ExistsByCodeAsync(companyId, request.Code, null, cancellationToken))
        {
            return Result<PaymentPeriodResponse>.Failure(PaymentErrors.PeriodCodeDuplicate);
        }

        var periodResult = PaymentPeriod.Create(
            Guid.NewGuid(),
            companyId,
            request.Code,
            request.StartsOn,
            request.EndsOn,
            clock.UtcNow);

        if (periodResult.IsFailure)
        {
            return Result<PaymentPeriodResponse>.Failure(periodResult.Error);
        }

        periodRepository.Add(periodResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PaymentPeriodResponse>.Success(PaymentPeriodResponse.FromDomain(periodResult.Value));
    }

    public async Task<Result> CloseAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result.Failure(PaymentErrors.InvalidCompanyId);
        }

        var period = await periodRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (period is null)
        {
            return Result.Failure(PaymentErrors.PeriodNotFound);
        }

        var closeResult = period.Close(clock.UtcNow);
        if (closeResult.IsFailure)
        {
            return closeResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> ReopenAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result.Failure(PaymentErrors.InvalidCompanyId);
        }

        var period = await periodRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (period is null)
        {
            return Result.Failure(PaymentErrors.PeriodNotFound);
        }

        var reopenResult = period.Reopen(clock.UtcNow);
        if (reopenResult.IsFailure)
        {
            return reopenResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
