using FluentValidation;
using NexoFleet.Application.Abstractions.Context;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Application.Abstractions.Time;
using NexoFleet.Application.Clients.Dtos;
using NexoFleet.Application.Common;
using NexoFleet.Domain.Clients;
using NexoFleet.Domain.Common;
using NexoFleet.Domain.Companies;

namespace NexoFleet.Application.Clients;

public sealed class ClientService(
    IClientRepository clientRepository,
    ICurrentTenant currentTenant,
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<CreateClientRequest> createValidator,
    IValidator<UpdateClientRequest> updateValidator)
{
    public async Task<Result<ClientResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<ClientResponse>.Failure(ClientErrors.InvalidCompanyId);
        }

        var client = await clientRepository.GetByIdAsync(companyId, id, cancellationToken);
        return client is null
            ? Result<ClientResponse>.Failure(ClientErrors.NotFound)
            : Result<ClientResponse>.Success(ClientResponse.FromDomain(client));
    }

    public async Task<Result<IReadOnlyList<ClientResponse>>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<IReadOnlyList<ClientResponse>>.Failure(ClientErrors.InvalidCompanyId);
        }

        var clients = await clientRepository.ListByCompanyIdAsync(companyId, cancellationToken);
        var responses = clients.Select(ClientResponse.FromDomain).ToArray();
        return Result<IReadOnlyList<ClientResponse>>.Success(responses);
    }

    public async Task<Result<ClientResponse>> CreateAsync(
        CreateClientRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<ClientResponse>.Failure(ClientErrors.InvalidCompanyId);
        }

        var validationResult = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<ClientResponse>.Failure(validationResult.ToValidationError());
        }

        if (await clientRepository.ExistsByCodeAsync(companyId, request.ClientCode, null, cancellationToken))
        {
            return Result<ClientResponse>.Failure(ClientErrors.ClientCodeDuplicate);
        }

        var codeResult = ClientCode.Create(request.ClientCode);
        if (codeResult.IsFailure) return Result<ClientResponse>.Failure(codeResult.Error);

        var nameResult = ClientName.Create(request.Name);
        if (nameResult.IsFailure) return Result<ClientResponse>.Failure(nameResult.Error);

        TaxIdentification? taxId = null;
        if (!string.IsNullOrWhiteSpace(request.TaxIdentification))
        {
            var taxResult = TaxIdentification.Create(request.TaxIdentification);
            if (taxResult.IsFailure) return Result<ClientResponse>.Failure(taxResult.Error);
            taxId = taxResult.Value;
        }

        ContactName? contactName = null;
        if (!string.IsNullOrWhiteSpace(request.ContactName))
        {
            var contactResult = ContactName.Create(request.ContactName);
            if (contactResult.IsFailure) return Result<ClientResponse>.Failure(contactResult.Error);
            contactName = contactResult.Value;
        }

        PhoneNumber? phone = null;
        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            var phoneResult = PhoneNumber.Create(request.Phone, ClientErrors.PhoneTooLong, ClientErrors.PhoneTooLong);
            if (phoneResult.IsFailure) return Result<ClientResponse>.Failure(phoneResult.Error);
            phone = phoneResult.Value;
        }

        Email? email = null;
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailResult = Email.Create(request.Email, ClientErrors.EmailInvalid, ClientErrors.EmailTooLong, ClientErrors.EmailInvalid);
            if (emailResult.IsFailure) return Result<ClientResponse>.Failure(emailResult.Error);
            email = emailResult.Value;
        }

        var now = clock.UtcNow;
        var clientResult = Client.Create(
            Guid.NewGuid(),
            companyId,
            codeResult.Value,
            nameResult.Value,
            taxId,
            contactName,
            phone,
            email,
            now);

        if (clientResult.IsFailure)
        {
            return Result<ClientResponse>.Failure(clientResult.Error);
        }

        clientRepository.Add(clientResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ClientResponse>.Success(ClientResponse.FromDomain(clientResult.Value));
    }

    public async Task<Result<ClientResponse>> UpdateProfileAsync(
        Guid id,
        UpdateClientRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<ClientResponse>.Failure(ClientErrors.InvalidCompanyId);
        }

        var validationResult = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<ClientResponse>.Failure(validationResult.ToValidationError());
        }

        var client = await clientRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (client is null)
        {
            return Result<ClientResponse>.Failure(ClientErrors.NotFound);
        }

        if (await clientRepository.ExistsByCodeAsync(companyId, request.ClientCode, id, cancellationToken))
        {
            return Result<ClientResponse>.Failure(ClientErrors.ClientCodeDuplicate);
        }

        var codeResult = ClientCode.Create(request.ClientCode);
        if (codeResult.IsFailure) return Result<ClientResponse>.Failure(codeResult.Error);

        var nameResult = ClientName.Create(request.Name);
        if (nameResult.IsFailure) return Result<ClientResponse>.Failure(nameResult.Error);

        TaxIdentification? taxId = null;
        if (!string.IsNullOrWhiteSpace(request.TaxIdentification))
        {
            var taxResult = TaxIdentification.Create(request.TaxIdentification);
            if (taxResult.IsFailure) return Result<ClientResponse>.Failure(taxResult.Error);
            taxId = taxResult.Value;
        }

        ContactName? contactName = null;
        if (!string.IsNullOrWhiteSpace(request.ContactName))
        {
            var contactResult = ContactName.Create(request.ContactName);
            if (contactResult.IsFailure) return Result<ClientResponse>.Failure(contactResult.Error);
            contactName = contactResult.Value;
        }

        PhoneNumber? phone = null;
        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            var phoneResult = PhoneNumber.Create(request.Phone, ClientErrors.PhoneTooLong, ClientErrors.PhoneTooLong);
            if (phoneResult.IsFailure) return Result<ClientResponse>.Failure(phoneResult.Error);
            phone = phoneResult.Value;
        }

        Email? email = null;
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailResult = Email.Create(request.Email, ClientErrors.EmailInvalid, ClientErrors.EmailTooLong, ClientErrors.EmailInvalid);
            if (emailResult.IsFailure) return Result<ClientResponse>.Failure(emailResult.Error);
            email = emailResult.Value;
        }

        var updateResult = client.UpdateProfile(
            codeResult.Value,
            nameResult.Value,
            taxId,
            contactName,
            phone,
            email,
            clock.UtcNow);

        if (updateResult.IsFailure)
        {
            return Result<ClientResponse>.Failure(updateResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ClientResponse>.Success(ClientResponse.FromDomain(client));
    }

    public async Task<Result> ActivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result.Failure(ClientErrors.InvalidCompanyId);
        }

        var client = await clientRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (client is null)
        {
            return Result.Failure(ClientErrors.NotFound);
        }

        var activateResult = client.Activate(clock.UtcNow);
        if (activateResult.IsFailure)
        {
            return activateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result.Failure(ClientErrors.InvalidCompanyId);
        }

        var client = await clientRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (client is null)
        {
            return Result.Failure(ClientErrors.NotFound);
        }

        var deactivateResult = client.Deactivate(clock.UtcNow);
        if (deactivateResult.IsFailure)
        {
            return deactivateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
