using NexoFleet.Domain.Companies;

namespace NexoFleet.Domain.UnitTests.Companies;

public sealed class AddressTests
{
    [Theory]
    [InlineData(null, "La Paz")]
    [InlineData("", "La Paz")]
    [InlineData("   ", "La Paz")]
    public void CreateShouldFailWhenCountryIsNullOrWhiteSpace(string? country, string? city)
    {
        var result = Address.Create(country, city);

        Assert.True(result.IsFailure);
        Assert.Equal(CompanyErrors.CountryRequired, result.Error);
    }

    [Fact]
    public void CreateShouldFailWhenCountryExceedsMaxLength()
    {
        var tooLongCountry = new string('A', Address.CountryMaxLength + 1);

        var result = Address.Create(tooLongCountry, "La Paz");

        Assert.True(result.IsFailure);
        Assert.Equal(CompanyErrors.CountryTooLong, result.Error);
    }

    [Theory]
    [InlineData("Bolivia", null)]
    [InlineData("Bolivia", "")]
    [InlineData("Bolivia", "   ")]
    public void CreateShouldFailWhenCityIsNullOrWhiteSpace(string? country, string? city)
    {
        var result = Address.Create(country, city);

        Assert.True(result.IsFailure);
        Assert.Equal(CompanyErrors.CityRequired, result.Error);
    }

    [Fact]
    public void CreateShouldFailWhenCityExceedsMaxLength()
    {
        var tooLongCity = new string('A', Address.CityMaxLength + 1);

        var result = Address.Create("Bolivia", tooLongCity);

        Assert.True(result.IsFailure);
        Assert.Equal(CompanyErrors.CityTooLong, result.Error);
    }

    [Fact]
    public void CreateShouldTrimAndSucceedWhenValid()
    {
        var result = Address.Create("  Bolivia  ", "  La Paz  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Bolivia", result.Value.Country);
        Assert.Equal("La Paz", result.Value.City);
    }
}
