using FluentValidation;
using NexoFleet.Application.Abstractions.Context;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Application.Abstractions.Time;
using NexoFleet.Application.Common;
using NexoFleet.Application.Routes.Dtos;
using NexoFleet.Domain.Clients;
using NexoFleet.Domain.Common;
using NexoFleet.Domain.Routes;

namespace NexoFleet.Application.Routes;

public sealed class RouteService(
    IRouteRepository routeRepository,
    IClientRepository clientRepository,
    ICurrentTenant currentTenant,
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<CreateRouteRequest> createValidator,
    IValidator<UpdateRouteDetailsRequest> updateValidator,
    IValidator<AddRouteStopRequest> addStopValidator,
    IValidator<UpdateRouteStopRequest> updateStopValidator)
{
    public async Task<Result<RouteResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<RouteResponse>.Failure(RouteErrors.InvalidCompanyId);
        }

        var route = await routeRepository.GetByIdAsync(companyId, id, cancellationToken);
        return route is null
            ? Result<RouteResponse>.Failure(RouteErrors.NotFound)
            : Result<RouteResponse>.Success(RouteResponse.FromDomain(route));
    }

    public async Task<Result<IReadOnlyList<RouteResponse>>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<IReadOnlyList<RouteResponse>>.Failure(RouteErrors.InvalidCompanyId);
        }

        var routes = await routeRepository.ListByCompanyIdAsync(companyId, cancellationToken);
        var responses = routes.Select(RouteResponse.FromDomain).ToArray();
        return Result<IReadOnlyList<RouteResponse>>.Success(responses);
    }

    public async Task<Result<RouteResponse>> CreateAsync(
        CreateRouteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<RouteResponse>.Failure(RouteErrors.InvalidCompanyId);
        }

        var validationResult = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<RouteResponse>.Failure(validationResult.ToValidationError());
        }

        if (request.ClientId.HasValue)
        {
            var client = await clientRepository.GetByIdAsync(companyId, request.ClientId.Value, cancellationToken);
            if (client is null)
            {
                return Result<RouteResponse>.Failure(ClientErrors.NotFound);
            }
        }

        if (await routeRepository.ExistsByRouteCodeAsync(companyId, request.RouteCode, null, cancellationToken))
        {
            return Result<RouteResponse>.Failure(RouteErrors.RouteCodeDuplicate);
        }

        var originResult = RouteLocation.Create(request.Origin.Address, request.Origin.Latitude, request.Origin.Longitude);
        if (originResult.IsFailure) return Result<RouteResponse>.Failure(originResult.Error);

        var destResult = RouteLocation.Create(request.Destination.Address, request.Destination.Latitude, request.Destination.Longitude);
        if (destResult.IsFailure) return Result<RouteResponse>.Failure(destResult.Error);

        var routeResult = Route.Create(
            Guid.NewGuid(),
            companyId,
            request.ClientId,
            request.RouteCode,
            request.Name,
            originResult.Value,
            destResult.Value,
            request.Instructions,
            request.EstimatedDurationMinutes,
            request.ReferenceAmount,
            request.ReferenceCurrency,
            clock.UtcNow);

        if (routeResult.IsFailure)
        {
            return Result<RouteResponse>.Failure(routeResult.Error);
        }

        routeRepository.Add(routeResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RouteResponse>.Success(RouteResponse.FromDomain(routeResult.Value));
    }

    public async Task<Result<RouteResponse>> UpdateDetailsAsync(
        Guid id,
        UpdateRouteDetailsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<RouteResponse>.Failure(RouteErrors.InvalidCompanyId);
        }

        var validationResult = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<RouteResponse>.Failure(validationResult.ToValidationError());
        }

        var route = await routeRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (route is null)
        {
            return Result<RouteResponse>.Failure(RouteErrors.NotFound);
        }

        if (request.ClientId.HasValue)
        {
            var client = await clientRepository.GetByIdAsync(companyId, request.ClientId.Value, cancellationToken);
            if (client is null)
            {
                return Result<RouteResponse>.Failure(ClientErrors.NotFound);
            }
        }

        if (await routeRepository.ExistsByRouteCodeAsync(companyId, request.RouteCode, id, cancellationToken))
        {
            return Result<RouteResponse>.Failure(RouteErrors.RouteCodeDuplicate);
        }

        var originResult = RouteLocation.Create(request.Origin.Address, request.Origin.Latitude, request.Origin.Longitude);
        if (originResult.IsFailure) return Result<RouteResponse>.Failure(originResult.Error);

        var destResult = RouteLocation.Create(request.Destination.Address, request.Destination.Latitude, request.Destination.Longitude);
        if (destResult.IsFailure) return Result<RouteResponse>.Failure(destResult.Error);

        var updateResult = route.UpdateDetails(
            request.ClientId,
            request.RouteCode,
            request.Name,
            originResult.Value,
            destResult.Value,
            request.Instructions,
            request.EstimatedDurationMinutes,
            request.ReferenceAmount,
            request.ReferenceCurrency,
            clock.UtcNow);

        if (updateResult.IsFailure)
        {
            return Result<RouteResponse>.Failure(updateResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<RouteResponse>.Success(RouteResponse.FromDomain(route));
    }

    public async Task<Result<RouteResponse>> AddStopAsync(
        Guid id,
        AddRouteStopRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<RouteResponse>.Failure(RouteErrors.InvalidCompanyId);
        }

        var validationResult = await addStopValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<RouteResponse>.Failure(validationResult.ToValidationError());
        }

        var route = await routeRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (route is null)
        {
            return Result<RouteResponse>.Failure(RouteErrors.NotFound);
        }

        var locationResult = RouteLocation.Create(request.Location.Address, request.Location.Latitude, request.Location.Longitude);
        if (locationResult.IsFailure) return Result<RouteResponse>.Failure(locationResult.Error);

        var addResult = route.AddStop(
            Guid.NewGuid(),
            locationResult.Value,
            request.Instructions,
            clock.UtcNow);

        if (addResult.IsFailure)
        {
            return Result<RouteResponse>.Failure(addResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<RouteResponse>.Success(RouteResponse.FromDomain(route));
    }

    public async Task<Result<RouteResponse>> UpdateStopAsync(
        Guid id,
        Guid stopId,
        UpdateRouteStopRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<RouteResponse>.Failure(RouteErrors.InvalidCompanyId);
        }

        var validationResult = await updateStopValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<RouteResponse>.Failure(validationResult.ToValidationError());
        }

        var route = await routeRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (route is null)
        {
            return Result<RouteResponse>.Failure(RouteErrors.NotFound);
        }

        var locationResult = RouteLocation.Create(request.Location.Address, request.Location.Latitude, request.Location.Longitude);
        if (locationResult.IsFailure) return Result<RouteResponse>.Failure(locationResult.Error);

        var updateResult = route.UpdateStop(
            stopId,
            locationResult.Value,
            request.Instructions,
            clock.UtcNow);

        if (updateResult.IsFailure)
        {
            return Result<RouteResponse>.Failure(updateResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<RouteResponse>.Success(RouteResponse.FromDomain(route));
    }

    public async Task<Result<RouteResponse>> MoveStopAsync(
        Guid id,
        Guid stopId,
        int newSequence,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<RouteResponse>.Failure(RouteErrors.InvalidCompanyId);
        }

        var route = await routeRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (route is null)
        {
            return Result<RouteResponse>.Failure(RouteErrors.NotFound);
        }

        var moveResult = route.MoveStop(stopId, newSequence, clock.UtcNow);
        if (moveResult.IsFailure)
        {
            return Result<RouteResponse>.Failure(moveResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<RouteResponse>.Success(RouteResponse.FromDomain(route));
    }

    public async Task<Result<RouteResponse>> RemoveStopAsync(
        Guid id,
        Guid stopId,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result<RouteResponse>.Failure(RouteErrors.InvalidCompanyId);
        }

        var route = await routeRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (route is null)
        {
            return Result<RouteResponse>.Failure(RouteErrors.NotFound);
        }

        var removeResult = route.RemoveStop(stopId, clock.UtcNow);
        if (removeResult.IsFailure)
        {
            return Result<RouteResponse>.Failure(removeResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<RouteResponse>.Success(RouteResponse.FromDomain(route));
    }

    public async Task<Result> ActivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (currentTenant.CompanyId is not { } companyId)
        {
            return Result.Failure(RouteErrors.InvalidCompanyId);
        }

        var route = await routeRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (route is null)
        {
            return Result.Failure(RouteErrors.NotFound);
        }

        var activateResult = route.Activate(clock.UtcNow);
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
            return Result.Failure(RouteErrors.InvalidCompanyId);
        }

        var route = await routeRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (route is null)
        {
            return Result.Failure(RouteErrors.NotFound);
        }

        var deactivateResult = route.Deactivate(clock.UtcNow);
        if (deactivateResult.IsFailure)
        {
            return deactivateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
