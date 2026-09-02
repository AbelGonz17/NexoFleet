namespace NexoFleet.Application.Routes.Dtos;

public sealed record UpdateRouteStopRequest(
    RouteLocationDto Location,
    string? Instructions = null);
