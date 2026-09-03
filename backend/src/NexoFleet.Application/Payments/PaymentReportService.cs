using FluentValidation;
using NexoFleet.Application.Abstractions.Context;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Application.Abstractions.Time;
using NexoFleet.Application.Common;
using NexoFleet.Application.Payments.Dtos;
using NexoFleet.Domain.Common;
using NexoFleet.Domain.Employees;
using NexoFleet.Domain.Payments;
using NexoFleet.Domain.Trips;

namespace NexoFleet.Application.Payments;

public sealed class PaymentReportService(
    IPaymentReportRepository reportRepository,
    IPaymentPeriodRepository periodRepository,
    IEmployeeRepository employeeRepository,
    ITripRepository tripRepository,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<CreatePaymentReportRequest> createValidator,
    IValidator<UpdatePaymentReportBaseAmountRequest> updateBaseAmountValidator,
    IValidator<AddPaymentItemRequest> addItemValidator,
    IValidator<UpdatePaymentItemRequest> updateItemValidator,
    IValidator<AddPaymentCommentRequest> addCommentValidator,
    IValidator<AddPaymentReportFileRequest> addFileValidator,
    IValidator<VoidPaymentReportRequest> voidValidator)
{
    public async Task<Result<PaymentReportResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<PaymentReportResponse>.Failure(PaymentErrors.InvalidCompanyId);
        }

        var report = await reportRepository.GetByIdAsync(companyId, id, cancellationToken);
        return report is null
            ? Result<PaymentReportResponse>.Failure(PaymentErrors.ReportNotFound)
            : Result<PaymentReportResponse>.Success(PaymentReportResponse.FromDomain(report));
    }

    public async Task<Result<IReadOnlyList<PaymentReportResponse>>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<IReadOnlyList<PaymentReportResponse>>.Failure(PaymentErrors.InvalidCompanyId);
        }

        var reports = await reportRepository.ListByCompanyIdAsync(companyId, cancellationToken);
        var responses = reports.Select(PaymentReportResponse.FromDomain).ToArray();
        return Result<IReadOnlyList<PaymentReportResponse>>.Success(responses);
    }

    public async Task<Result<IReadOnlyList<PaymentReportResponse>>> ListByPeriodIdAsync(
        Guid periodId,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<IReadOnlyList<PaymentReportResponse>>.Failure(PaymentErrors.InvalidCompanyId);
        }

        var reports = await reportRepository.ListByPeriodIdAsync(companyId, periodId, cancellationToken);
        var responses = reports.Select(PaymentReportResponse.FromDomain).ToArray();
        return Result<IReadOnlyList<PaymentReportResponse>>.Success(responses);
    }

    public async Task<Result<PaymentReportResponse>> CreateAsync(
        CreatePaymentReportRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<PaymentReportResponse>.Failure(PaymentErrors.InvalidCompanyId);
        }

        var validationResult = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<PaymentReportResponse>.Failure(validationResult.ToValidationError());
        }

        var period = await periodRepository.GetByIdAsync(companyId, request.PaymentPeriodId, cancellationToken);
        if (period is null) return Result<PaymentReportResponse>.Failure(PaymentErrors.PeriodNotFound);

        var employee = await employeeRepository.GetByIdAsync(companyId, request.EmployeeId, cancellationToken);
        if (employee is null) return Result<PaymentReportResponse>.Failure(EmployeeErrors.NotFound);

        var existing = await reportRepository.GetByPeriodAndEmployeeAsync(companyId, request.PaymentPeriodId, request.EmployeeId, cancellationToken);
        if (existing is not null) return Result<PaymentReportResponse>.Failure(PaymentErrors.ReportAlreadyExists);

        var reportResult = PaymentReport.Create(
            Guid.NewGuid(),
            companyId,
            request.PaymentPeriodId,
            request.EmployeeId,
            request.BaseAmount,
            request.Currency,
            clock.UtcNow);

        if (reportResult.IsFailure)
        {
            return Result<PaymentReportResponse>.Failure(reportResult.Error);
        }

        reportRepository.Add(reportResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PaymentReportResponse>.Success(PaymentReportResponse.FromDomain(reportResult.Value));
    }

    public async Task<Result<PaymentReportResponse>> UpdateBaseAmountAsync(
        Guid id,
        UpdatePaymentReportBaseAmountRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<PaymentReportResponse>.Failure(PaymentErrors.InvalidCompanyId);
        }

        var validationResult = await updateBaseAmountValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<PaymentReportResponse>.Failure(validationResult.ToValidationError());
        }

        var report = await reportRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (report is null) return Result<PaymentReportResponse>.Failure(PaymentErrors.ReportNotFound);

        var updateResult = report.UpdateBaseAmount(request.BaseAmount, request.Currency, clock.UtcNow);
        if (updateResult.IsFailure)
        {
            return Result<PaymentReportResponse>.Failure(updateResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<PaymentReportResponse>.Success(PaymentReportResponse.FromDomain(report));
    }

    public async Task<Result<PaymentReportResponse>> AddItemAsync(
        Guid id,
        AddPaymentItemRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<PaymentReportResponse>.Failure(PaymentErrors.InvalidCompanyId);
        }

        var validationResult = await addItemValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<PaymentReportResponse>.Failure(validationResult.ToValidationError());
        }

        var report = await reportRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (report is null) return Result<PaymentReportResponse>.Failure(PaymentErrors.ReportNotFound);

        if (request.TripId.HasValue)
        {
            var trip = await tripRepository.GetByIdAsync(companyId, request.TripId.Value, cancellationToken);
            if (trip is null) return Result<PaymentReportResponse>.Failure(TripErrors.NotFound);
        }

        var addResult = report.AddItem(
            Guid.NewGuid(),
            request.TripId,
            request.Effect,
            request.Description,
            request.Amount,
            clock.UtcNow);

        if (addResult.IsFailure)
        {
            return Result<PaymentReportResponse>.Failure(addResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<PaymentReportResponse>.Success(PaymentReportResponse.FromDomain(report));
    }

    public async Task<Result<PaymentReportResponse>> UpdateItemAsync(
        Guid id,
        Guid itemId,
        UpdatePaymentItemRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<PaymentReportResponse>.Failure(PaymentErrors.InvalidCompanyId);
        }

        var validationResult = await updateItemValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<PaymentReportResponse>.Failure(validationResult.ToValidationError());
        }

        var report = await reportRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (report is null) return Result<PaymentReportResponse>.Failure(PaymentErrors.ReportNotFound);

        var updateResult = report.UpdateItem(itemId, request.Effect, request.Description, request.Amount, clock.UtcNow);
        if (updateResult.IsFailure)
        {
            return Result<PaymentReportResponse>.Failure(updateResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<PaymentReportResponse>.Success(PaymentReportResponse.FromDomain(report));
    }

    public async Task<Result<PaymentReportResponse>> RemoveItemAsync(
        Guid id,
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<PaymentReportResponse>.Failure(PaymentErrors.InvalidCompanyId);
        }

        var report = await reportRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (report is null) return Result<PaymentReportResponse>.Failure(PaymentErrors.ReportNotFound);

        var removeResult = report.RemoveItem(itemId, clock.UtcNow);
        if (removeResult.IsFailure)
        {
            return Result<PaymentReportResponse>.Failure(removeResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<PaymentReportResponse>.Success(PaymentReportResponse.FromDomain(report));
    }

    public async Task<Result<PaymentReportResponse>> AddCommentAsync(
        Guid id,
        AddPaymentCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<PaymentReportResponse>.Failure(PaymentErrors.InvalidCompanyId);
        }

        if (currentUser.UserId is not { } authorUserId)
        {
            return Result<PaymentReportResponse>.Failure(PaymentErrors.InvalidUserId);
        }

        var validationResult = await addCommentValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<PaymentReportResponse>.Failure(validationResult.ToValidationError());
        }

        var report = await reportRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (report is null) return Result<PaymentReportResponse>.Failure(PaymentErrors.ReportNotFound);

        var addCommentResult = report.AddComment(Guid.NewGuid(), authorUserId, request.Text, clock.UtcNow);
        if (addCommentResult.IsFailure)
        {
            return Result<PaymentReportResponse>.Failure(addCommentResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<PaymentReportResponse>.Success(PaymentReportResponse.FromDomain(report));
    }

    public async Task<Result<PaymentReportResponse>> AddFileAsync(
        Guid id,
        AddPaymentReportFileRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<PaymentReportResponse>.Failure(PaymentErrors.InvalidCompanyId);
        }

        if (currentUser.UserId is not { } uploadedByUserId)
        {
            return Result<PaymentReportResponse>.Failure(PaymentErrors.InvalidUserId);
        }

        var validationResult = await addFileValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<PaymentReportResponse>.Failure(validationResult.ToValidationError());
        }

        var report = await reportRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (report is null) return Result<PaymentReportResponse>.Failure(PaymentErrors.ReportNotFound);

        var addFileResult = report.AddFile(
            Guid.NewGuid(),
            request.FileName,
            request.StorageKey,
            request.ContentType,
            request.SizeInBytes,
            uploadedByUserId,
            clock.UtcNow);

        if (addFileResult.IsFailure)
        {
            return Result<PaymentReportResponse>.Failure(addFileResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<PaymentReportResponse>.Success(PaymentReportResponse.FromDomain(report));
    }

    public async Task<Result<PaymentReportResponse>> PublishAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<PaymentReportResponse>.Failure(PaymentErrors.InvalidCompanyId);
        }

        var report = await reportRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (report is null) return Result<PaymentReportResponse>.Failure(PaymentErrors.ReportNotFound);

        var publishResult = report.Publish(clock.UtcNow);
        if (publishResult.IsFailure)
        {
            return Result<PaymentReportResponse>.Failure(publishResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<PaymentReportResponse>.Success(PaymentReportResponse.FromDomain(report));
    }

    public async Task<Result<PaymentReportResponse>> VoidAsync(
        Guid id,
        VoidPaymentReportRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<PaymentReportResponse>.Failure(PaymentErrors.InvalidCompanyId);
        }

        var validationResult = await voidValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<PaymentReportResponse>.Failure(validationResult.ToValidationError());
        }

        var report = await reportRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (report is null) return Result<PaymentReportResponse>.Failure(PaymentErrors.ReportNotFound);

        var voidResult = report.Void(request.Reason, clock.UtcNow);
        if (voidResult.IsFailure)
        {
            return Result<PaymentReportResponse>.Failure(voidResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<PaymentReportResponse>.Success(PaymentReportResponse.FromDomain(report));
    }
}
