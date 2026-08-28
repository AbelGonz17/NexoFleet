using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Routes;

public sealed record RouteLocation
{
    public const int AddressMaxLength = 300;
    public const int CoordinatePrecision = 9;
    public const int CoordinateScale = 6;

    private RouteLocation(
        string address,
        decimal? latitude,
        decimal? longitude)
    {
        Address = address;
        Latitude = latitude;
        Longitude = longitude;
    }

    private RouteLocation()
    {
    }

    public string Address { get; private set; } = string.Empty;

    public decimal? Latitude { get; private set; }

    public decimal? Longitude { get; private set; }

    public static Result<RouteLocation> Create(
        string address,
        decimal? latitude = null,
        decimal? longitude = null)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return Result<RouteLocation>.Failure(RouteLocationErrors.AddressRequired);
        }

        if (address.Trim().Length > AddressMaxLength)
        {
            return Result<RouteLocation>.Failure(RouteLocationErrors.AddressTooLong);
        }

        if (latitude.HasValue != longitude.HasValue)
        {
            return Result<RouteLocation>.Failure(RouteLocationErrors.CoordinatesIncomplete);
        }

        if (latitude is < -90 or > 90)
        {
            return Result<RouteLocation>.Failure(RouteLocationErrors.LatitudeOutOfRange);
        }

        if (longitude is < -180 or > 180)
        {
            return Result<RouteLocation>.Failure(RouteLocationErrors.LongitudeOutOfRange);
        }

        return Result<RouteLocation>.Success(new RouteLocation(
            address.Trim(),
            latitude,
            longitude));
    }
}
