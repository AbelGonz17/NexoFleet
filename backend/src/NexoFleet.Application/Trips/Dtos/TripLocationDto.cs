using NexoFleet.Domain.Routes;

namespace NexoFleet.Application.Trips.Dtos;

public sealed record TripLocationDto(
    string Address,
    decimal? Latitude = null,
    decimal? Longitude = null)
{
    public static TripLocationDto FromDomain(RouteLocation location) =>
        new(location.Address, location.Latitude, location.Longitude);
}
