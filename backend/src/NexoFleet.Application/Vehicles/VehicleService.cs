using FluentValidation;
using NexoFleet.Application.Abstractions.Context;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Application.Abstractions.Time;
using NexoFleet.Application.Common;
using NexoFleet.Application.Vehicles.Dtos;
using NexoFleet.Domain.Common;
using NexoFleet.Domain.Employees;
using NexoFleet.Domain.Vehicles;

namespace NexoFleet.Application.Vehicles;

public sealed class VehicleService(
    IVehicleRepository vehicleRepository,
    IEmployeeRepository employeeRepository,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<RegisterCompanyVehicleRequest> registerCompanyValidator,
    IValidator<RegisterEmployeeVehicleRequest> registerEmployeeValidator,
    IValidator<UpdateVehicleDetailsRequest> updateDetailsValidator,
    IValidator<RejectVehicleRequest> rejectValidator,
    IValidator<AddVehicleDocumentRequest> addDocumentValidator)
{
    public async Task<Result<VehicleResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<VehicleResponse>.Failure(VehicleErrors.InvalidCompanyId);
        }

        var vehicle = await vehicleRepository.GetByIdAsync(companyId, id, cancellationToken);
        return vehicle is null
            ? Result<VehicleResponse>.Failure(VehicleErrors.NotFound)
            : Result<VehicleResponse>.Success(VehicleResponse.FromDomain(vehicle));
    }

    public async Task<Result<IReadOnlyList<VehicleResponse>>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<IReadOnlyList<VehicleResponse>>.Failure(VehicleErrors.InvalidCompanyId);
        }

        var vehicles = await vehicleRepository.ListByCompanyIdAsync(companyId, cancellationToken);
        var responses = vehicles.Select(VehicleResponse.FromDomain).ToArray();
        return Result<IReadOnlyList<VehicleResponse>>.Success(responses);
    }

    public async Task<Result<IReadOnlyList<VehicleResponse>>> GetByOwnerEmployeeIdAsync(
        Guid ownerEmployeeId,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<IReadOnlyList<VehicleResponse>>.Failure(VehicleErrors.InvalidCompanyId);
        }

        var vehicles = await vehicleRepository.GetByOwnerEmployeeIdAsync(companyId, ownerEmployeeId, cancellationToken);
        var responses = vehicles.Select(VehicleResponse.FromDomain).ToArray();
        return Result<IReadOnlyList<VehicleResponse>>.Success(responses);
    }

    public async Task<Result<VehicleResponse>> RegisterCompanyVehicleAsync(
        RegisterCompanyVehicleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<VehicleResponse>.Failure(VehicleErrors.InvalidCompanyId);
        }

        var validationResult = await registerCompanyValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<VehicleResponse>.Failure(validationResult.ToValidationError());
        }

        if (await vehicleRepository.ExistsByLicensePlateAsync(companyId, request.LicensePlate, null, cancellationToken))
        {
            return Result<VehicleResponse>.Failure(VehicleErrors.LicensePlateDuplicate);
        }

        var vehicleResult = Vehicle.CreateCompanyOwned(
            Guid.NewGuid(),
            companyId,
            request.LicensePlate,
            request.Make,
            request.Model,
            request.ManufactureYear,
            request.Color,
            request.Type,
            request.PassengerCapacity,
            clock.UtcNow);

        if (vehicleResult.IsFailure)
        {
            return Result<VehicleResponse>.Failure(vehicleResult.Error);
        }

        vehicleRepository.Add(vehicleResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<VehicleResponse>.Success(VehicleResponse.FromDomain(vehicleResult.Value));
    }

    public async Task<Result<VehicleResponse>> RegisterEmployeeVehicleAsync(
        RegisterEmployeeVehicleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<VehicleResponse>.Failure(VehicleErrors.InvalidCompanyId);
        }

        var validationResult = await registerEmployeeValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<VehicleResponse>.Failure(validationResult.ToValidationError());
        }

        var owner = await employeeRepository.GetByIdAsync(companyId, request.OwnerEmployeeId, cancellationToken);
        if (owner is null)
        {
            return Result<VehicleResponse>.Failure(EmployeeErrors.NotFound);
        }

        if (await vehicleRepository.ExistsByLicensePlateAsync(companyId, request.LicensePlate, null, cancellationToken))
        {
            return Result<VehicleResponse>.Failure(VehicleErrors.LicensePlateDuplicate);
        }

        var vehicleResult = Vehicle.CreateEmployeeOwned(
            Guid.NewGuid(),
            companyId,
            request.OwnerEmployeeId,
            request.LicensePlate,
            request.Make,
            request.Model,
            request.ManufactureYear,
            request.Color,
            request.Type,
            request.PassengerCapacity,
            clock.UtcNow);

        if (vehicleResult.IsFailure)
        {
            return Result<VehicleResponse>.Failure(vehicleResult.Error);
        }

        vehicleRepository.Add(vehicleResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<VehicleResponse>.Success(VehicleResponse.FromDomain(vehicleResult.Value));
    }

    public async Task<Result<VehicleResponse>> UpdateDetailsAsync(
        Guid id,
        UpdateVehicleDetailsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<VehicleResponse>.Failure(VehicleErrors.InvalidCompanyId);
        }

        var validationResult = await updateDetailsValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<VehicleResponse>.Failure(validationResult.ToValidationError());
        }

        var vehicle = await vehicleRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (vehicle is null)
        {
            return Result<VehicleResponse>.Failure(VehicleErrors.NotFound);
        }

        if (await vehicleRepository.ExistsByLicensePlateAsync(companyId, request.LicensePlate, id, cancellationToken))
        {
            return Result<VehicleResponse>.Failure(VehicleErrors.LicensePlateDuplicate);
        }

        var updateResult = vehicle.UpdateDetails(
            request.LicensePlate,
            request.Make,
            request.Model,
            request.ManufactureYear,
            request.Color,
            request.Type,
            request.PassengerCapacity,
            clock.UtcNow);

        if (updateResult.IsFailure)
        {
            return Result<VehicleResponse>.Failure(updateResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<VehicleResponse>.Success(VehicleResponse.FromDomain(vehicle));
    }

    public async Task<Result> ApproveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result.Failure(VehicleErrors.InvalidCompanyId);
        }

        var vehicle = await vehicleRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (vehicle is null)
        {
            return Result.Failure(VehicleErrors.NotFound);
        }

        var approveResult = vehicle.Approve(clock.UtcNow);
        if (approveResult.IsFailure)
        {
            return approveResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> RejectAsync(
        Guid id,
        RejectVehicleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result.Failure(VehicleErrors.InvalidCompanyId);
        }

        var validationResult = await rejectValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure(validationResult.ToValidationError());
        }

        var vehicle = await vehicleRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (vehicle is null)
        {
            return Result.Failure(VehicleErrors.NotFound);
        }

        var rejectResult = vehicle.Reject(request.Reason, clock.UtcNow);
        if (rejectResult.IsFailure)
        {
            return rejectResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<VehicleResponse>> AddDocumentAsync(
        Guid id,
        AddVehicleDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<VehicleResponse>.Failure(VehicleErrors.InvalidCompanyId);
        }

        if (currentUser.UserId is not { } userId)
        {
            return Result<VehicleResponse>.Failure(VehicleErrors.InvalidUploadedByUserId);
        }

        var validationResult = await addDocumentValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<VehicleResponse>.Failure(validationResult.ToValidationError());
        }

        var vehicle = await vehicleRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (vehicle is null)
        {
            return Result<VehicleResponse>.Failure(VehicleErrors.NotFound);
        }

        var docId = Guid.NewGuid();
        var addResult = vehicle.AddDocument(
            docId,
            request.Type,
            request.FileName,
            request.StorageKey,
            request.ContentType,
            request.SizeInBytes,
            request.ExpiresOn,
            userId,
            clock.UtcNow);

        if (addResult.IsFailure)
        {
            return Result<VehicleResponse>.Failure(addResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<VehicleResponse>.Success(VehicleResponse.FromDomain(vehicle));
    }

    public async Task<Result> RemoveDocumentAsync(
        Guid id,
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result.Failure(VehicleErrors.InvalidCompanyId);
        }

        var vehicle = await vehicleRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (vehicle is null)
        {
            return Result.Failure(VehicleErrors.NotFound);
        }

        var removeResult = vehicle.RemoveDocument(documentId, clock.UtcNow);
        if (removeResult.IsFailure)
        {
            return removeResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> SendToMaintenanceAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result.Failure(VehicleErrors.InvalidCompanyId);
        }

        var vehicle = await vehicleRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (vehicle is null)
        {
            return Result.Failure(VehicleErrors.NotFound);
        }

        var maintenanceResult = vehicle.SendToMaintenance(clock.UtcNow);
        if (maintenanceResult.IsFailure)
        {
            return maintenanceResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> ReturnToOperationalAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result.Failure(VehicleErrors.InvalidCompanyId);
        }

        var vehicle = await vehicleRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (vehicle is null)
        {
            return Result.Failure(VehicleErrors.NotFound);
        }

        var operationalResult = vehicle.ReturnToOperational(clock.UtcNow);
        if (operationalResult.IsFailure)
        {
            return operationalResult;
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
            return Result.Failure(VehicleErrors.InvalidCompanyId);
        }

        var vehicle = await vehicleRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (vehicle is null)
        {
            return Result.Failure(VehicleErrors.NotFound);
        }

        var retireResult = vehicle.Retire(clock.UtcNow);
        if (retireResult.IsFailure)
        {
            return retireResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
