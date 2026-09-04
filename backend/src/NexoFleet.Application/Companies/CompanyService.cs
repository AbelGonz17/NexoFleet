using FluentValidation;
using NexoFleet.Application.Abstractions.Authentication;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Application.Abstractions.Time;
using NexoFleet.Application.Authentication;
using NexoFleet.Application.Authorization;
using NexoFleet.Application.Common;
using NexoFleet.Application.Companies.Dtos;
using NexoFleet.Domain.Common;
using NexoFleet.Domain.Companies;

namespace NexoFleet.Application.Companies;

public sealed class CompanyService(
    ICompanyRepository companyRepository,
    IIdentityService identityService,
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<CreateCompanyRequest> createValidator,
    IValidator<UpdateCompanyProfileRequest> updateValidator,
    IValidator<CreateCompanyAdminRequest> createAdminValidator)
{
    public async Task<Result<CompanyResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var company = await companyRepository.GetByIdAsync(id, cancellationToken);
        return company is null
            ? Result<CompanyResponse>.Failure(CompanyErrors.NotFound)
            : Result<CompanyResponse>.Success(CompanyResponse.FromDomain(company));
    }

    public async Task<Result<IReadOnlyList<CompanyResponse>>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var companies = await companyRepository.ListAsync(cancellationToken);
        var responses = companies.Select(CompanyResponse.FromDomain).ToArray();
        return Result<IReadOnlyList<CompanyResponse>>.Success(responses);
    }

    public async Task<Result<CompanyResponse>> CreateAsync(
        CreateCompanyRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<CompanyResponse>.Failure(validationResult.ToValidationError());
        }

        if (await companyRepository.ExistsByTaxIdentificationAsync(request.TaxIdentification, null, cancellationToken))
        {
            return Result<CompanyResponse>.Failure(CompanyErrors.TaxIdentificationDuplicate);
        }

        var nameResult = CompanyName.Create(request.Name);
        if (nameResult.IsFailure) return Result<CompanyResponse>.Failure(nameResult.Error);

        var taxIdResult = TaxIdentification.Create(request.TaxIdentification);
        if (taxIdResult.IsFailure) return Result<CompanyResponse>.Failure(taxIdResult.Error);

        var addressResult = Address.Create(request.Country, request.City);
        if (addressResult.IsFailure) return Result<CompanyResponse>.Failure(addressResult.Error);

        var phoneResult = PhoneNumber.Create(request.Phone, CompanyErrors.PhoneRequired, CompanyErrors.PhoneTooLong);
        if (phoneResult.IsFailure) return Result<CompanyResponse>.Failure(phoneResult.Error);

        var emailResult = Email.Create(request.Email, CompanyErrors.EmailInvalid, CompanyErrors.EmailTooLong, CompanyErrors.EmailRequired);
        if (emailResult.IsFailure) return Result<CompanyResponse>.Failure(emailResult.Error);

        var now = clock.UtcNow;
        var companyResult = Company.Create(
            Guid.NewGuid(),
            nameResult.Value,
            taxIdResult.Value,
            addressResult.Value,
            phoneResult.Value,
            emailResult.Value,
            now);

        if (companyResult.IsFailure)
        {
            return Result<CompanyResponse>.Failure(companyResult.Error);
        }

        companyRepository.Add(companyResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CompanyResponse>.Success(CompanyResponse.FromDomain(companyResult.Value));
    }

    public async Task<Result<CompanyResponse>> UpdateProfileAsync(
        Guid id,
        UpdateCompanyProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<CompanyResponse>.Failure(validationResult.ToValidationError());
        }

        var company = await companyRepository.GetByIdAsync(id, cancellationToken);
        if (company is null)
        {
            return Result<CompanyResponse>.Failure(CompanyErrors.NotFound);
        }

        if (await companyRepository.ExistsByTaxIdentificationAsync(request.TaxIdentification, id, cancellationToken))
        {
            return Result<CompanyResponse>.Failure(CompanyErrors.TaxIdentificationDuplicate);
        }

        var nameResult = CompanyName.Create(request.Name);
        if (nameResult.IsFailure) return Result<CompanyResponse>.Failure(nameResult.Error);

        var taxIdResult = TaxIdentification.Create(request.TaxIdentification);
        if (taxIdResult.IsFailure) return Result<CompanyResponse>.Failure(taxIdResult.Error);

        var addressResult = Address.Create(request.Country, request.City);
        if (addressResult.IsFailure) return Result<CompanyResponse>.Failure(addressResult.Error);

        var phoneResult = PhoneNumber.Create(request.Phone, CompanyErrors.PhoneRequired, CompanyErrors.PhoneTooLong);
        if (phoneResult.IsFailure) return Result<CompanyResponse>.Failure(phoneResult.Error);

        var emailResult = Email.Create(request.Email, CompanyErrors.EmailInvalid, CompanyErrors.EmailTooLong, CompanyErrors.EmailRequired);
        if (emailResult.IsFailure) return Result<CompanyResponse>.Failure(emailResult.Error);

        var updateResult = company.UpdateProfile(
            nameResult.Value,
            taxIdResult.Value,
            addressResult.Value,
            phoneResult.Value,
            emailResult.Value,
            clock.UtcNow);

        if (updateResult.IsFailure)
        {
            return Result<CompanyResponse>.Failure(updateResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<CompanyResponse>.Success(CompanyResponse.FromDomain(company));
    }

    public async Task<Result> SuspendAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var company = await companyRepository.GetByIdAsync(id, cancellationToken);
        if (company is null)
        {
            return Result.Failure(CompanyErrors.NotFound);
        }

        var suspendResult = company.Suspend(clock.UtcNow);
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
        var company = await companyRepository.GetByIdAsync(id, cancellationToken);
        if (company is null)
        {
            return Result.Failure(CompanyErrors.NotFound);
        }

        var activateResult = company.Activate(clock.UtcNow);
        if (activateResult.IsFailure)
        {
            return activateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<AuthenticatedUser>> CreateAdminAsync(
        Guid companyId,
        CreateCompanyAdminRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await createAdminValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<AuthenticatedUser>.Failure(validationResult.ToValidationError());
        }

        var company = await companyRepository.GetByIdAsync(companyId, cancellationToken);
        if (company is null)
        {
            return Result<AuthenticatedUser>.Failure(CompanyErrors.NotFound);
        }

        return await identityService.CreateUserAsync(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            companyId,
            UserRoles.Administrator,
            cancellationToken);
    }

    public async Task<Result<IReadOnlyList<AuthenticatedUser>>> ListAdminsAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        var company = await companyRepository.GetByIdAsync(companyId, cancellationToken);
        if (company is null)
        {
            return Result<IReadOnlyList<AuthenticatedUser>>.Failure(CompanyErrors.NotFound);
        }

        var users = await identityService.GetUsersByCompanyIdAsync(companyId, cancellationToken);
        return Result<IReadOnlyList<AuthenticatedUser>>.Success(users);
    }
}
