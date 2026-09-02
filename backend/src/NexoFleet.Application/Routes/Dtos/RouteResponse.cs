using NexoFleet.Domain.Routes;

namespace NexoFleet.Application.Routes.Dtos;

public sealed record RouteResponse(
    Guid Id,
    Guid CompanyId,
    Guid? ClientId,
    string RouteCode,
    string Name,
    RouteLocationDto Origin,
    RouteLocationDto Destination,
    string? Instructions,
    int? EstimatedDurationMinutes,
    decimal? ReferenceAmount,
    string? ReferenceCurrency,
    string Status,
    IReadOnlyList<RouteStopResponse> Stops,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc)
{
    public static RouteResponse FromDomain(Route route) =>
        new(
            route.Id,
            route.CompanyId,
            route.ClientId,
            route.RouteCode,
            route.Name,
            RouteLocationDto.FromDomain(route.Origin),
            RouteLocationDto.FromDomain(route.Destination),
            route.Instructions,
            route.EstimatedDurationMinutes,
            route.ReferenceAmount,
            route.ReferenceCurrency,
            route.Status.ToString(),
            route.Stops.OrderBy(stop => stop.Sequence).Select(RouteStopResponse.FromDomain).ToArray(),
            route.CreatedAtUtc,
            route.UpdatedAtUtc);
}
