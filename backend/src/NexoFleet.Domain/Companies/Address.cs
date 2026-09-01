using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.Companies;

public sealed record Address
{
    public const int CountryMaxLength = 100;
    public const int CityMaxLength = 100;

    public string Country { get; } = string.Empty;
    public string City { get; } = string.Empty;

    private Address(string country, string city)
    {
        Country = country;
        City = city;
    }

    private Address() { }

    public static Result<Address> Create(string? country, string? city)
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            return Result<Address>.Failure(CompanyErrors.CountryRequired);
        }

        var trimmedCountry = country.Trim();
        if (trimmedCountry.Length > CountryMaxLength)
        {
            return Result<Address>.Failure(CompanyErrors.CountryTooLong);
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            return Result<Address>.Failure(CompanyErrors.CityRequired);
        }

        var trimmedCity = city.Trim();
        if (trimmedCity.Length > CityMaxLength)
        {
            return Result<Address>.Failure(CompanyErrors.CityTooLong);
        }

        return Result<Address>.Success(new Address(trimmedCountry, trimmedCity));
    }

    public override string ToString() => $"{City}, {Country}";
}
