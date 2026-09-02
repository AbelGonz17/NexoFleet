using FluentValidation;
using NexoFleet.Application.Abstractions.Context;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Application.Abstractions.Time;
using NexoFleet.Application.Common;
using NexoFleet.Application.RouteSchedules.Dtos;
using NexoFleet.Domain.Common;
using NexoFleet.Domain.Employees;
using NexoFleet.Domain.Routes;
using NexoFleet.Domain.RouteSchedules;
using NexoFleet.Domain.Vehicles;

namespace NexoFleet.Application.RouteSchedules;

public sealed class RouteScheduleService(
    IRouteScheduleRepository routeScheduleRepository,
    IRouteRepository routeRepository,
    IEmployeeRepository employeeRepository,
    IVehicleRepository vehicleRepository,
    ICurrentTenant currentTenant,
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<CreateRouteScheduleRequest> createValidator,
    IValidator<ConfigureRouteScheduleRecurrenceRequest> recurrenceValidator,
    IValidator<AssignScheduleResourcesRequest> assignValidator,
    IValidator<EndCurrentScheduleAssignmentRequest> endAssignmentValidator)
{
    public async Task<Result<RouteScheduleResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<RouteScheduleResponse>.Failure(RouteScheduleErrors.InvalidCompanyId);
        }

        var schedule = await routeScheduleRepository.GetByIdAsync(companyId, id, cancellationToken);
        return schedule is null
            ? Result<RouteScheduleResponse>.Failure(RouteScheduleErrors.NotFound)
            : Result<RouteScheduleResponse>.Success(RouteScheduleResponse.FromDomain(schedule));
    }

    public async Task<Result<IReadOnlyList<RouteScheduleResponse>>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<IReadOnlyList<RouteScheduleResponse>>.Failure(RouteScheduleErrors.InvalidCompanyId);
        }

        var schedules = await routeScheduleRepository.ListByCompanyIdAsync(companyId, cancellationToken);
        var responses = schedules.Select(RouteScheduleResponse.FromDomain).ToArray();
        return Result<IReadOnlyList<RouteScheduleResponse>>.Success(responses);
    }

    public async Task<Result<IReadOnlyList<RouteScheduleResponse>>> GetByRouteIdAsync(
        Guid routeId,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<IReadOnlyList<RouteScheduleResponse>>.Failure(RouteScheduleErrors.InvalidCompanyId);
        }

        var schedules = await routeScheduleRepository.GetByRouteIdAsync(companyId, routeId, cancellationToken);
        var responses = schedules.Select(RouteScheduleResponse.FromDomain).ToArray();
        return Result<IReadOnlyList<RouteScheduleResponse>>.Success(responses);
    }

    public async Task<Result<RouteScheduleResponse>> CreateAsync(
        CreateRouteScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<RouteScheduleResponse>.Failure(RouteScheduleErrors.InvalidCompanyId);
        }

        var validationResult = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<RouteScheduleResponse>.Failure(validationResult.ToValidationError());
        }

        var route = await routeRepository.GetByIdAsync(companyId, request.RouteId, cancellationToken);
        if (route is null)
        {
            return Result<RouteScheduleResponse>.Failure(RouteErrors.NotFound);
        }

        var scheduleResult = RouteSchedule.Create(
            Guid.NewGuid(),
            companyId,
            request.RouteId,
            request.Shift,
            request.StartTime,
            request.EndTime,
            request.Days,
            request.EffectiveFrom,
            request.EffectiveUntil,
            request.DefaultAmount,
            request.DefaultCurrency,
            clock.UtcNow);

        if (scheduleResult.IsFailure)
        {
            return Result<RouteScheduleResponse>.Failure(scheduleResult.Error);
        }

        routeScheduleRepository.Add(scheduleResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RouteScheduleResponse>.Success(RouteScheduleResponse.FromDomain(scheduleResult.Value));
    }

    public async Task<Result<RouteScheduleResponse>> ConfigureRecurrenceAsync(
        Guid id,
        ConfigureRouteScheduleRecurrenceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<RouteScheduleResponse>.Failure(RouteScheduleErrors.InvalidCompanyId);
        }

        var validationResult = await recurrenceValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<RouteScheduleResponse>.Failure(validationResult.ToValidationError());
        }

        var schedule = await routeScheduleRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (schedule is null)
        {
            return Result<RouteScheduleResponse>.Failure(RouteScheduleErrors.NotFound);
        }

        var configureResult = schedule.ConfigureRecurrence(
            request.Shift,
            request.StartTime,
            request.EndTime,
            request.Days,
            request.EffectiveFrom,
            request.EffectiveUntil,
            request.DefaultAmount,
            request.DefaultCurrency,
            clock.UtcNow);

        if (configureResult.IsFailure)
        {
            return Result<RouteScheduleResponse>.Failure(configureResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<RouteScheduleResponse>.Success(RouteScheduleResponse.FromDomain(schedule));
    }

    public async Task<Result<RouteScheduleResponse>> AssignResourcesAsync(
        Guid id,
        AssignScheduleResourcesRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<RouteScheduleResponse>.Failure(RouteScheduleErrors.InvalidCompanyId);
        }

        var validationResult = await assignValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<RouteScheduleResponse>.Failure(validationResult.ToValidationError());
        }

        var schedule = await routeScheduleRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (schedule is null)
        {
            return Result<RouteScheduleResponse>.Failure(RouteScheduleErrors.NotFound);
        }

        var employee = await employeeRepository.GetByIdAsync(companyId, request.EmployeeId, cancellationToken);
        if (employee is null)
        {
            return Result<RouteScheduleResponse>.Failure(EmployeeErrors.NotFound);
        }

        if (request.VehicleId.HasValue)
        {
            var vehicle = await vehicleRepository.GetByIdAsync(companyId, request.VehicleId.Value, cancellationToken);
            if (vehicle is null)
            {
                return Result<RouteScheduleResponse>.Failure(VehicleErrors.NotFound);
            }
        }

        var assignResult = schedule.AssignResources(
            Guid.NewGuid(),
            request.EmployeeId,
            request.VehicleId,
            request.ValidFrom,
            request.ValidUntil,
            clock.UtcNow);

        if (assignResult.IsFailure)
        {
            return Result<RouteScheduleResponse>.Failure(assignResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<RouteScheduleResponse>.Success(RouteScheduleResponse.FromDomain(schedule));
    }

    public async Task<Result<RouteScheduleResponse>> EndCurrentAssignmentAsync(
        Guid id,
        EndCurrentScheduleAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<RouteScheduleResponse>.Failure(RouteScheduleErrors.InvalidCompanyId);
        }

        var validationResult = await endAssignmentValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<RouteScheduleResponse>.Failure(validationResult.ToValidationError());
        }

        var schedule = await routeScheduleRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (schedule is null)
        {
            return Result<RouteScheduleResponse>.Failure(RouteScheduleErrors.NotFound);
        }

        var endResult = schedule.EndCurrentAssignment(request.ValidUntil, clock.UtcNow);
        if (endResult.IsFailure)
        {
            return Result<RouteScheduleResponse>.Failure(endResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<RouteScheduleResponse>.Success(RouteScheduleResponse.FromDomain(schedule));
    }

    public async Task<Result> ActivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result.Failure(RouteScheduleErrors.InvalidCompanyId);
        }

        var schedule = await routeScheduleRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (schedule is null)
        {
            return Result.Failure(RouteScheduleErrors.NotFound);
        }

        var activateResult = schedule.Activate(clock.UtcNow);
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
            return Result.Failure(RouteScheduleErrors.InvalidCompanyId);
        }

        var schedule = await routeScheduleRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (schedule is null)
        {
            return Result.Failure(RouteScheduleErrors.NotFound);
        }

        var deactivateResult = schedule.Deactivate(clock.UtcNow);
        if (deactivateResult.IsFailure)
        {
            return deactivateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
