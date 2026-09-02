using NexoFleet.Domain.Routes;

namespace NexoFleet.Application.Routes.Dtos;

public sealed record RouteStopResponse(
    Guid Id,
    Guid RouteId,
    int Sequence,
    RouteLocationDto Location,
    string? Instructions)
{
    public static RouteStopResponse FromDomain(RouteStop stop) =>
        new(
            stop.Id,
            stop.RouteId,
            stop.Sequence,
            RouteLocationDto.FromDomain(stop.Location),
            stop.Instructions);
}
