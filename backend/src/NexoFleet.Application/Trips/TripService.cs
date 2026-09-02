using FluentValidation;
using NexoFleet.Application.Abstractions.Context;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Application.Abstractions.Time;
using NexoFleet.Application.Common;
using NexoFleet.Application.Trips.Dtos;
using NexoFleet.Domain.Clients;
using NexoFleet.Domain.Common;
using NexoFleet.Domain.Employees;
using NexoFleet.Domain.Routes;
using NexoFleet.Domain.RouteSchedules;
using NexoFleet.Domain.Trips;
using NexoFleet.Domain.Vehicles;

namespace NexoFleet.Application.Trips;

public sealed class TripService(
    ITripRepository tripRepository,
    IClientRepository clientRepository,
    IRouteRepository routeRepository,
    IRouteScheduleRepository routeScheduleRepository,
    IEmployeeRepository employeeRepository,
    IVehicleRepository vehicleRepository,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<CreatePlannedTripRequest> createPlannedValidator,
    IValidator<SubmitUnexpectedTripRequest> submitUnexpectedValidator,
    IValidator<UpdateTripPlanRequest> updatePlanValidator,
    IValidator<ApproveTripRequest> approveValidator,
    IValidator<RejectTripRequest> rejectValidator,
    IValidator<AssignTripRequest> assignValidator,
    IValidator<CompleteTripRequest> completeValidator,
    IValidator<CancelTripRequest> cancelValidator,
    IValidator<AddTripIncidentRequest> addIncidentValidator,
    IValidator<AddTripFileRequest> addFileValidator)
{
    public async Task<Result<TripResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<TripResponse>.Failure(TripErrors.InvalidCompanyId);
        }

        var trip = await tripRepository.GetByIdAsync(companyId, id, cancellationToken);
        return trip is null
            ? Result<TripResponse>.Failure(TripErrors.NotFound)
            : Result<TripResponse>.Success(TripResponse.FromDomain(trip));
    }

    public async Task<Result<IReadOnlyList<TripResponse>>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<IReadOnlyList<TripResponse>>.Failure(TripErrors.InvalidCompanyId);
        }

        var trips = await tripRepository.ListByCompanyIdAsync(companyId, cancellationToken);
        var responses = trips.Select(TripResponse.FromDomain).ToArray();
        return Result<IReadOnlyList<TripResponse>>.Success(responses);
    }

    public async Task<Result<TripResponse>> CreatePlannedAsync(
        CreatePlannedTripRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<TripResponse>.Failure(TripErrors.InvalidCompanyId);
        }

        var validationResult = await createPlannedValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<TripResponse>.Failure(validationResult.ToValidationError());
        }

        if (await tripRepository.ExistsByNumberAsync(companyId, request.TripNumber, cancellationToken))
        {
            return Result<TripResponse>.Failure(TripErrors.TripNumberDuplicate);
        }

        if (request.ClientId.HasValue)
        {
            var client = await clientRepository.GetByIdAsync(companyId, request.ClientId.Value, cancellationToken);
            if (client is null) return Result<TripResponse>.Failure(ClientErrors.NotFound);
        }

        if (request.RouteId.HasValue)
        {
            var route = await routeRepository.GetByIdAsync(companyId, request.RouteId.Value, cancellationToken);
            if (route is null) return Result<TripResponse>.Failure(RouteErrors.NotFound);
        }

        if (request.RouteScheduleId.HasValue)
        {
            var schedule = await routeScheduleRepository.GetByIdAsync(companyId, request.RouteScheduleId.Value, cancellationToken);
            if (schedule is null) return Result<TripResponse>.Failure(RouteScheduleErrors.NotFound);
        }

        var originResult = RouteLocation.Create(request.Origin.Address, request.Origin.Latitude, request.Origin.Longitude);
        if (originResult.IsFailure) return Result<TripResponse>.Failure(originResult.Error);

        var destResult = RouteLocation.Create(request.Destination.Address, request.Destination.Latitude, request.Destination.Longitude);
        if (destResult.IsFailure) return Result<TripResponse>.Failure(destResult.Error);

        var tripResult = Trip.CreatePlanned(
            Guid.NewGuid(),
            companyId,
            request.TripNumber,
            request.ClientId,
            request.RouteId,
            request.RouteScheduleId,
            request.ServiceDate,
            originResult.Value,
            destResult.Value,
            request.AgreedAmount,
            request.Currency,
            clock.UtcNow);

        if (tripResult.IsFailure)
        {
            return Result<TripResponse>.Failure(tripResult.Error);
        }

        tripRepository.Add(tripResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TripResponse>.Success(TripResponse.FromDomain(tripResult.Value));
    }

    public async Task<Result<TripResponse>> SubmitUnexpectedAsync(
        SubmitUnexpectedTripRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<TripResponse>.Failure(TripErrors.InvalidCompanyId);
        }

        var validationResult = await submitUnexpectedValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<TripResponse>.Failure(validationResult.ToValidationError());
        }

        if (await tripRepository.ExistsByNumberAsync(companyId, request.TripNumber, cancellationToken))
        {
            return Result<TripResponse>.Failure(TripErrors.TripNumberDuplicate);
        }

        var employee = await employeeRepository.GetByIdAsync(companyId, request.SubmittedByEmployeeId, cancellationToken);
        if (employee is null) return Result<TripResponse>.Failure(EmployeeErrors.NotFound);

        if (request.ClientId.HasValue)
        {
            var client = await clientRepository.GetByIdAsync(companyId, request.ClientId.Value, cancellationToken);
            if (client is null) return Result<TripResponse>.Failure(ClientErrors.NotFound);
        }

        if (request.RouteId.HasValue)
        {
            var route = await routeRepository.GetByIdAsync(companyId, request.RouteId.Value, cancellationToken);
            if (route is null) return Result<TripResponse>.Failure(RouteErrors.NotFound);
        }

        var originResult = RouteLocation.Create(request.Origin.Address, request.Origin.Latitude, request.Origin.Longitude);
        if (originResult.IsFailure) return Result<TripResponse>.Failure(originResult.Error);

        var destResult = RouteLocation.Create(request.Destination.Address, request.Destination.Latitude, request.Destination.Longitude);
        if (destResult.IsFailure) return Result<TripResponse>.Failure(destResult.Error);

        var tripResult = Trip.SubmitUnexpected(
            Guid.NewGuid(),
            companyId,
            request.TripNumber,
            request.SubmittedByEmployeeId,
            request.ClientId,
            request.RouteId,
            request.ServiceDate,
            originResult.Value,
            destResult.Value,
            request.ProposedAmount,
            request.Currency,
            clock.UtcNow);

        if (tripResult.IsFailure)
        {
            return Result<TripResponse>.Failure(tripResult.Error);
        }

        tripRepository.Add(tripResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TripResponse>.Success(TripResponse.FromDomain(tripResult.Value));
    }

    public async Task<Result<TripResponse>> UpdatePlanAsync(
        Guid id,
        UpdateTripPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<TripResponse>.Failure(TripErrors.InvalidCompanyId);
        }

        var validationResult = await updatePlanValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<TripResponse>.Failure(validationResult.ToValidationError());
        }

        var trip = await tripRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (trip is null) return Result<TripResponse>.Failure(TripErrors.NotFound);

        if (request.ClientId.HasValue)
        {
            var client = await clientRepository.GetByIdAsync(companyId, request.ClientId.Value, cancellationToken);
            if (client is null) return Result<TripResponse>.Failure(ClientErrors.NotFound);
        }

        if (request.RouteId.HasValue)
        {
            var route = await routeRepository.GetByIdAsync(companyId, request.RouteId.Value, cancellationToken);
            if (route is null) return Result<TripResponse>.Failure(RouteErrors.NotFound);
        }

        var originResult = RouteLocation.Create(request.Origin.Address, request.Origin.Latitude, request.Origin.Longitude);
        if (originResult.IsFailure) return Result<TripResponse>.Failure(originResult.Error);

        var destResult = RouteLocation.Create(request.Destination.Address, request.Destination.Latitude, request.Destination.Longitude);
        if (destResult.IsFailure) return Result<TripResponse>.Failure(destResult.Error);

        var updateResult = trip.UpdatePlan(
            request.ClientId,
            request.RouteId,
            request.ServiceDate,
            originResult.Value,
            destResult.Value,
            request.AgreedAmount,
            request.Currency,
            clock.UtcNow);

        if (updateResult.IsFailure)
        {
            return Result<TripResponse>.Failure(updateResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<TripResponse>.Success(TripResponse.FromDomain(trip));
    }

    public async Task<Result<TripResponse>> ApproveAsync(
        Guid id,
        ApproveTripRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<TripResponse>.Failure(TripErrors.InvalidCompanyId);
        }

        if (currentUser.UserId is not { } reviewerUserId)
        {
            return Result<TripResponse>.Failure(TripErrors.InvalidUserId);
        }

        var validationResult = await approveValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<TripResponse>.Failure(validationResult.ToValidationError());
        }

        var trip = await tripRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (trip is null) return Result<TripResponse>.Failure(TripErrors.NotFound);

        var approveResult = trip.Approve(Guid.NewGuid(), reviewerUserId, request.Comments, clock.UtcNow);
        if (approveResult.IsFailure)
        {
            return Result<TripResponse>.Failure(approveResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<TripResponse>.Success(TripResponse.FromDomain(trip));
    }

    public async Task<Result<TripResponse>> RejectAsync(
        Guid id,
        RejectTripRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<TripResponse>.Failure(TripErrors.InvalidCompanyId);
        }

        if (currentUser.UserId is not { } reviewerUserId)
        {
            return Result<TripResponse>.Failure(TripErrors.InvalidUserId);
        }

        var validationResult = await rejectValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<TripResponse>.Failure(validationResult.ToValidationError());
        }

        var trip = await tripRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (trip is null) return Result<TripResponse>.Failure(TripErrors.NotFound);

        var rejectResult = trip.Reject(Guid.NewGuid(), reviewerUserId, request.Reason, clock.UtcNow);
        if (rejectResult.IsFailure)
        {
            return Result<TripResponse>.Failure(rejectResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<TripResponse>.Success(TripResponse.FromDomain(trip));
    }

    public async Task<Result<TripResponse>> AssignAsync(
        Guid id,
        AssignTripRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<TripResponse>.Failure(TripErrors.InvalidCompanyId);
        }

        if (currentUser.UserId is not { } assignedByUserId)
        {
            return Result<TripResponse>.Failure(TripErrors.InvalidUserId);
        }

        var validationResult = await assignValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<TripResponse>.Failure(validationResult.ToValidationError());
        }

        var trip = await tripRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (trip is null) return Result<TripResponse>.Failure(TripErrors.NotFound);

        var employee = await employeeRepository.GetByIdAsync(companyId, request.EmployeeId, cancellationToken);
        if (employee is null) return Result<TripResponse>.Failure(EmployeeErrors.NotFound);

        if (request.VehicleId.HasValue)
        {
            var vehicle = await vehicleRepository.GetByIdAsync(companyId, request.VehicleId.Value, cancellationToken);
            if (vehicle is null) return Result<TripResponse>.Failure(VehicleErrors.NotFound);
        }

        var assignResult = trip.Assign(
            Guid.NewGuid(),
            request.EmployeeId,
            request.VehicleId,
            assignedByUserId,
            clock.UtcNow);

        if (assignResult.IsFailure)
        {
            return Result<TripResponse>.Failure(assignResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<TripResponse>.Success(TripResponse.FromDomain(trip));
    }

    public async Task<Result<TripResponse>> StartAsync(
        Guid id,
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<TripResponse>.Failure(TripErrors.InvalidCompanyId);
        }

        var trip = await tripRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (trip is null) return Result<TripResponse>.Failure(TripErrors.NotFound);

        var startResult = trip.Start(employeeId, clock.UtcNow);
        if (startResult.IsFailure)
        {
            return Result<TripResponse>.Failure(startResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<TripResponse>.Success(TripResponse.FromDomain(trip));
    }

    public async Task<Result<TripResponse>> CompleteAsync(
        Guid id,
        Guid employeeId,
        CompleteTripRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<TripResponse>.Failure(TripErrors.InvalidCompanyId);
        }

        var validationResult = await completeValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<TripResponse>.Failure(validationResult.ToValidationError());
        }

        var trip = await tripRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (trip is null) return Result<TripResponse>.Failure(TripErrors.NotFound);

        var completeResult = trip.Complete(
            employeeId,
            request.FinalAmount,
            request.Currency,
            clock.UtcNow);

        if (completeResult.IsFailure)
        {
            return Result<TripResponse>.Failure(completeResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<TripResponse>.Success(TripResponse.FromDomain(trip));
    }

    public async Task<Result<TripResponse>> CancelAsync(
        Guid id,
        CancelTripRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<TripResponse>.Failure(TripErrors.InvalidCompanyId);
        }

        var validationResult = await cancelValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<TripResponse>.Failure(validationResult.ToValidationError());
        }

        var trip = await tripRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (trip is null) return Result<TripResponse>.Failure(TripErrors.NotFound);

        var cancelResult = trip.Cancel(request.Reason, clock.UtcNow);
        if (cancelResult.IsFailure)
        {
            return Result<TripResponse>.Failure(cancelResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<TripResponse>.Success(TripResponse.FromDomain(trip));
    }

    public async Task<Result<TripResponse>> AddIncidentAsync(
        Guid id,
        AddTripIncidentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<TripResponse>.Failure(TripErrors.InvalidCompanyId);
        }

        var validationResult = await addIncidentValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<TripResponse>.Failure(validationResult.ToValidationError());
        }

        var trip = await tripRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (trip is null) return Result<TripResponse>.Failure(TripErrors.NotFound);

        var employee = await employeeRepository.GetByIdAsync(companyId, request.ReportedByEmployeeId, cancellationToken);
        if (employee is null) return Result<TripResponse>.Failure(EmployeeErrors.NotFound);

        var addIncidentResult = trip.AddIncident(
            Guid.NewGuid(),
            request.ReportedByEmployeeId,
            request.Severity,
            request.Description,
            request.IncidentAtUtc,
            clock.UtcNow);

        if (addIncidentResult.IsFailure)
        {
            return Result<TripResponse>.Failure(addIncidentResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<TripResponse>.Success(TripResponse.FromDomain(trip));
    }

    public async Task<Result<TripResponse>> AddFileAsync(
        Guid id,
        AddTripFileRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<TripResponse>.Failure(TripErrors.InvalidCompanyId);
        }

        if (currentUser.UserId is not { } uploadedByUserId)
        {
            return Result<TripResponse>.Failure(TripErrors.InvalidUserId);
        }

        var validationResult = await addFileValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<TripResponse>.Failure(validationResult.ToValidationError());
        }

        var trip = await tripRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (trip is null) return Result<TripResponse>.Failure(TripErrors.NotFound);

        var addFileResult = trip.AddFile(
            Guid.NewGuid(),
            request.FileName,
            request.StorageKey,
            request.ContentType,
            request.SizeInBytes,
            uploadedByUserId,
            clock.UtcNow);

        if (addFileResult.IsFailure)
        {
            return Result<TripResponse>.Failure(addFileResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<TripResponse>.Success(TripResponse.FromDomain(trip));
    }
}
