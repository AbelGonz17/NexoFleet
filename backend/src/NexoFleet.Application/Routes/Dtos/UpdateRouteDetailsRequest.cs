namespace NexoFleet.Application.Routes.Dtos;

public sealed record UpdateRouteDetailsRequest(
    string RouteCode,
    string Name,
    RouteLocationDto Origin,
    RouteLocationDto Destination,
    Guid? ClientId = null,
    string? Instructions = null,
    int? EstimatedDurationMinutes = null,
    decimal? ReferenceAmount = null,
    string? ReferenceCurrency = null);
