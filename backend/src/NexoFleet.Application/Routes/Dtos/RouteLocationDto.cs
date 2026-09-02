using NexoFleet.Domain.Routes;

namespace NexoFleet.Application.Routes.Dtos;

public sealed record RouteLocationDto(
    string Address,
    decimal? Latitude = null,
    decimal? Longitude = null)
{
    public static RouteLocationDto FromDomain(RouteLocation location) =>
        new(location.Address, location.Latitude, location.Longitude);
}
