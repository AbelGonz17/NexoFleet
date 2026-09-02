using FluentValidation;
using NexoFleet.Application.Abstractions.Context;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Application.Abstractions.Time;
using NexoFleet.Application.Common;
using NexoFleet.Application.Employees.Dtos;
using NexoFleet.Domain.Common;
using NexoFleet.Domain.Employees;

namespace NexoFleet.Application.Employees;

public sealed class EmployeeService(
    IEmployeeRepository employeeRepository,
    ICurrentTenant currentTenant,
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<CreateEmployeeRequest> createValidator,
    IValidator<UpdateEmployeeRequest> updateValidator,
    IValidator<LinkUserAccountRequest> linkUserValidator)
{
    public async Task<Result<EmployeeResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<EmployeeResponse>.Failure(EmployeeErrors.InvalidCompanyId);
        }

        var employee = await employeeRepository.GetByIdAsync(companyId, id, cancellationToken);
        return employee is null
            ? Result<EmployeeResponse>.Failure(EmployeeErrors.NotFound)
            : Result<EmployeeResponse>.Success(EmployeeResponse.FromDomain(employee));
    }

    public async Task<Result<IReadOnlyList<EmployeeResponse>>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<IReadOnlyList<EmployeeResponse>>.Failure(EmployeeErrors.InvalidCompanyId);
        }

        var employees = await employeeRepository.ListByCompanyIdAsync(companyId, cancellationToken);
        var responses = employees.Select(EmployeeResponse.FromDomain).ToArray();
        return Result<IReadOnlyList<EmployeeResponse>>.Success(responses);
    }

    public async Task<Result<EmployeeResponse>> CreateAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<EmployeeResponse>.Failure(EmployeeErrors.InvalidCompanyId);
        }

        var validationResult = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<EmployeeResponse>.Failure(validationResult.ToValidationError());
        }

        if (await employeeRepository.ExistsByEmployeeCodeAsync(companyId, request.EmployeeCode, null, cancellationToken))
        {
            return Result<EmployeeResponse>.Failure(EmployeeErrors.EmployeeCodeDuplicate);
        }

        if (await employeeRepository.ExistsByIdentityDocumentAsync(companyId, request.IdentityDocument, null, cancellationToken))
        {
            return Result<EmployeeResponse>.Failure(EmployeeErrors.IdentityDocumentDuplicate);
        }

        if (await employeeRepository.ExistsByEmailAsync(companyId, request.Email, null, cancellationToken))
        {
            return Result<EmployeeResponse>.Failure(EmployeeErrors.EmailDuplicate);
        }

        var codeResult = EmployeeCode.Create(request.EmployeeCode);
        if (codeResult.IsFailure) return Result<EmployeeResponse>.Failure(codeResult.Error);

        var fullNameResult = FullName.Create(request.FirstName, request.LastName);
        if (fullNameResult.IsFailure) return Result<EmployeeResponse>.Failure(fullNameResult.Error);

        var docResult = IdentityDocument.Create(request.IdentityDocument);
        if (docResult.IsFailure) return Result<EmployeeResponse>.Failure(docResult.Error);

        var phoneResult = PhoneNumber.Create(request.Phone, EmployeeErrors.PhoneRequired, EmployeeErrors.PhoneTooLong);
        if (phoneResult.IsFailure) return Result<EmployeeResponse>.Failure(phoneResult.Error);

        var emailResult = Email.Create(request.Email, EmployeeErrors.EmailInvalid, EmployeeErrors.EmailTooLong, EmployeeErrors.EmailInvalid);
        if (emailResult.IsFailure) return Result<EmployeeResponse>.Failure(emailResult.Error);

        var now = clock.UtcNow;
        var employeeResult = Employee.Create(
            Guid.NewGuid(),
            companyId,
            codeResult.Value,
            fullNameResult.Value,
            docResult.Value,
            phoneResult.Value,
            emailResult.Value,
            request.HireDate,
            now);

        if (employeeResult.IsFailure)
        {
            return Result<EmployeeResponse>.Failure(employeeResult.Error);
        }

        employeeRepository.Add(employeeResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<EmployeeResponse>.Success(EmployeeResponse.FromDomain(employeeResult.Value));
    }

    public async Task<Result<EmployeeResponse>> UpdateProfileAsync(
        Guid id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<EmployeeResponse>.Failure(EmployeeErrors.InvalidCompanyId);
        }

        var validationResult = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<EmployeeResponse>.Failure(validationResult.ToValidationError());
        }

        var employee = await employeeRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (employee is null)
        {
            return Result<EmployeeResponse>.Failure(EmployeeErrors.NotFound);
        }

        if (await employeeRepository.ExistsByEmployeeCodeAsync(companyId, request.EmployeeCode, id, cancellationToken))
        {
            return Result<EmployeeResponse>.Failure(EmployeeErrors.EmployeeCodeDuplicate);
        }

        if (await employeeRepository.ExistsByIdentityDocumentAsync(companyId, request.IdentityDocument, id, cancellationToken))
        {
            return Result<EmployeeResponse>.Failure(EmployeeErrors.IdentityDocumentDuplicate);
        }

        if (await employeeRepository.ExistsByEmailAsync(companyId, request.Email, id, cancellationToken))
        {
            return Result<EmployeeResponse>.Failure(EmployeeErrors.EmailDuplicate);
        }

        var codeResult = EmployeeCode.Create(request.EmployeeCode);
        if (codeResult.IsFailure) return Result<EmployeeResponse>.Failure(codeResult.Error);

        var fullNameResult = FullName.Create(request.FirstName, request.LastName);
        if (fullNameResult.IsFailure) return Result<EmployeeResponse>.Failure(fullNameResult.Error);

        var docResult = IdentityDocument.Create(request.IdentityDocument);
        if (docResult.IsFailure) return Result<EmployeeResponse>.Failure(docResult.Error);

        var phoneResult = PhoneNumber.Create(request.Phone, EmployeeErrors.PhoneRequired, EmployeeErrors.PhoneTooLong);
        if (phoneResult.IsFailure) return Result<EmployeeResponse>.Failure(phoneResult.Error);

        var emailResult = Email.Create(request.Email, EmployeeErrors.EmailInvalid, EmployeeErrors.EmailTooLong, EmployeeErrors.EmailInvalid);
        if (emailResult.IsFailure) return Result<EmployeeResponse>.Failure(emailResult.Error);

        var updateResult = employee.UpdateProfile(
            codeResult.Value,
            fullNameResult.Value,
            docResult.Value,
            phoneResult.Value,
            emailResult.Value,
            request.HireDate,
            clock.UtcNow);

        if (updateResult.IsFailure)
        {
            return Result<EmployeeResponse>.Failure(updateResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<EmployeeResponse>.Success(EmployeeResponse.FromDomain(employee));
    }

    public async Task<Result> LinkUserAccountAsync(
        Guid id,
        LinkUserAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result.Failure(EmployeeErrors.InvalidCompanyId);
        }

        var validationResult = await linkUserValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure(validationResult.ToValidationError());
        }

        var employee = await employeeRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (employee is null)
        {
            return Result.Failure(EmployeeErrors.NotFound);
        }

        var existingWithUser = await employeeRepository.GetByUserIdAsync(companyId, request.UserId, cancellationToken);
        if (existingWithUser is not null && existingWithUser.Id != id)
        {
            return Result.Failure(EmployeeErrors.UserAccountAlreadyLinked);
        }

        var linkResult = employee.LinkUserAccount(request.UserId, clock.UtcNow);
        if (linkResult.IsFailure)
        {
            return linkResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> UnlinkUserAccountAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result.Failure(EmployeeErrors.InvalidCompanyId);
        }

        var employee = await employeeRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (employee is null)
        {
            return Result.Failure(EmployeeErrors.NotFound);
        }

        var unlinkResult = employee.UnlinkUserAccount(clock.UtcNow);
        if (unlinkResult.IsFailure)
        {
            return unlinkResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> SuspendAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result.Failure(EmployeeErrors.InvalidCompanyId);
        }

        var employee = await employeeRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (employee is null)
        {
            return Result.Failure(EmployeeErrors.NotFound);
        }

        var suspendResult = employee.Suspend(clock.UtcNow);
        if (suspendResult.IsFailure)
        {
            return suspendResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> ActivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result.Failure(EmployeeErrors.InvalidCompanyId);
        }

        var employee = await employeeRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (employee is null)
        {
            return Result.Failure(EmployeeErrors.NotFound);
        }

        var activateResult = employee.Activate(clock.UtcNow);
        if (activateResult.IsFailure)
        {
            return activateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> RetireAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result.Failure(EmployeeErrors.InvalidCompanyId);
        }

        var employee = await employeeRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (employee is null)
        {
            return Result.Failure(EmployeeErrors.NotFound);
        }

        var retireResult = employee.Retire(clock.UtcNow);
        if (retireResult.IsFailure)
        {
            return retireResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
