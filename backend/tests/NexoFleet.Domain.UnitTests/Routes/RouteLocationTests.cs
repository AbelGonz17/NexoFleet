using NexoFleet.Domain.Routes;

namespace NexoFleet.Domain.UnitTests.Routes;

public sealed class RouteLocationTests
{
    [Fact]
    public void CreateShouldNormalizeAddressAndAllowOptionalCoordinates()
    {
        var addressOnlyResult = RouteLocation.Create(" Terminal Central ");
        var coordinatesResult = RouteLocation.Create(
            " Plaza Principal ",
            -16.489689m,
            -68.119293m);

        Assert.True(addressOnlyResult.IsSuccess);
        Assert.Equal("Terminal Central", addressOnlyResult.Value.Address);
        Assert.Null(addressOnlyResult.Value.Latitude);
        Assert.Null(addressOnlyResult.Value.Longitude);

        Assert.True(coordinatesResult.IsSuccess);
        Assert.Equal("Plaza Principal", coordinatesResult.Value.Address);
        Assert.Equal(-16.489689m, coordinatesResult.Value.Latitude);
        Assert.Equal(-68.119293m, coordinatesResult.Value.Longitude);
    }

    [Theory]
    [InlineData(null, -68.1, "RouteLocation.CoordinatesIncomplete")]
    [InlineData(-16.5, null, "RouteLocation.CoordinatesIncomplete")]
    [InlineData(-90.1, -68.1, "RouteLocation.LatitudeOutOfRange")]
    [InlineData(90.1, -68.1, "RouteLocation.LatitudeOutOfRange")]
    [InlineData(-16.5, -180.1, "RouteLocation.LongitudeOutOfRange")]
    [InlineData(-16.5, 180.1, "RouteLocation.LongitudeOutOfRange")]
    public void CreateShouldRejectInvalidCoordinates(
        double? latitude,
        double? longitude,
        string expectedErrorCode)
    {
        var result = RouteLocation.Create(
            "Ubicación",
            latitude.HasValue ? (decimal)latitude.Value : null,
            longitude.HasValue ? (decimal)longitude.Value : null);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedErrorCode, result.Error.Code);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-90, -180)]
    [InlineData(90, 180)]
    public void CreateShouldAllowCoordinateBoundaries(
        double latitude,
        double longitude)
    {
        var result = RouteLocation.Create(
            "Ubicación",
            (decimal)latitude,
            (decimal)longitude);

        Assert.True(result.IsSuccess);
    }
}
