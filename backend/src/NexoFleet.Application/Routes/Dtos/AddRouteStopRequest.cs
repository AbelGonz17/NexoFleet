namespace NexoFleet.Application.Routes.Dtos;

public sealed record AddRouteStopRequest(
    RouteLocationDto Location,
    string? Instructions = null);
