using NexoFleet.Domain.Companies;
using NexoFleet.Domain.Companies.Events;
using NexoFleet.Domain.Common;

namespace NexoFleet.Domain.UnitTests.Companies;

public sealed class CompanyTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateShouldNormalizeDataAndRaiseDomainEvent()
    {
        var id = Guid.NewGuid();

        var result = CreateCompany(id);

        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value.Id);
        Assert.Equal("Nexo Transport", result.Value.Name);
        Assert.Equal("BO-123-ABC", result.Value.TaxIdentification);
        Assert.Equal("contacto@nexo.test", result.Value.Email);
        Assert.Equal(CompanyStatus.Active, result.Value.Status);
        Assert.Equal(Now, result.Value.CreatedAtUtc);
        Assert.Single(result.Value.DomainEvents);
        Assert.IsType<CompanyCreatedDomainEvent>(result.Value.DomainEvents.Single());
    }

    [Theory]
    [InlineData("", "BO-123", "Bolivia", "La Paz", "+59170000000", "contacto@nexo.test", "Company.NameRequired")]
    [InlineData("Nexo", "", "Bolivia", "La Paz", "+59170000000", "contacto@nexo.test", "Company.TaxIdentificationRequired")]
    [InlineData("Nexo", "BO-123", "", "La Paz", "+59170000000", "contacto@nexo.test", "Company.CountryRequired")]
    [InlineData("Nexo", "BO-123", "Bolivia", "", "+59170000000", "contacto@nexo.test", "Company.CityRequired")]
    [InlineData("Nexo", "BO-123", "Bolivia", "La Paz", "", "contacto@nexo.test", "Company.PhoneRequired")]
    [InlineData("Nexo", "BO-123", "Bolivia", "La Paz", "+59170000000", "invalid", "Company.EmailInvalid")]
    public void CreateShouldRejectInvalidProfile(
        string name,
        string taxIdentification,
        string country,
        string city,
        string phone,
        string email,
        string expectedErrorCode)
    {
        var result = Company.Create(
            Guid.NewGuid(),
            name,
            taxIdentification,
            country,
            city,
            phone,
            email,
            Now);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedErrorCode, result.Error.Code);
    }

    [Fact]
    public void UpdateProfileShouldChangeDataAndRaiseDomainEvent()
    {
        var company = CreateCompany().Value;
        company.ClearDomainEvents();

        var result = company.UpdateProfile(
            "Nexo Fleet SRL",
            "bo-456",
            "Bolivia",
            "Santa Cruz",
            "+59171111111",
            "ADMIN@NEXO.TEST",
            Now.AddHours(1));

        Assert.True(result.IsSuccess);
        Assert.Equal("BO-456", company.TaxIdentification);
        Assert.Equal("admin@nexo.test", company.Email);
        Assert.Equal(Now.AddHours(1), company.UpdatedAtUtc);
        Assert.IsType<CompanyProfileUpdatedDomainEvent>(company.DomainEvents.Single());
    }

    [Fact]
    public void UpdateProfileShouldNotRaiseEventWhenNothingChanged()
    {
        var company = CreateCompany().Value;
        company.ClearDomainEvents();

        var result = company.UpdateProfile(
            company.Name,
            company.TaxIdentification,
            company.Country,
            company.City,
            company.Phone,
            company.Email,
            Now.AddHours(1));

        Assert.True(result.IsSuccess);
        Assert.Null(company.UpdatedAtUtc);
        Assert.Empty(company.DomainEvents);
    }

    [Fact]
    public void SuspendAndActivateShouldControlCompanyStatus()
    {
        var company = CreateCompany().Value;
        company.ClearDomainEvents();

        var suspendResult = company.Suspend(Now.AddHours(1));
        var activateResult = company.Activate(Now.AddHours(2));

        Assert.True(suspendResult.IsSuccess);
        Assert.True(activateResult.IsSuccess);
        Assert.Equal(CompanyStatus.Active, company.Status);
        Assert.Equal(2, company.DomainEvents.Count);
        var lastEvent = Assert.IsType<CompanyStatusChangedDomainEvent>(company.DomainEvents.Last());
        Assert.Equal(CompanyStatus.Suspended, lastEvent.PreviousStatus);
        Assert.Equal(CompanyStatus.Active, lastEvent.CurrentStatus);
    }

    [Fact]
    public void SuspendShouldFailWhenCompanyIsAlreadySuspended()
    {
        var company = CreateCompany().Value;
        company.Suspend(Now.AddHours(1));

        var result = company.Suspend(Now.AddHours(2));

        Assert.True(result.IsFailure);
        Assert.Equal(CompanyErrors.AlreadySuspended, result.Error);
    }

    private static Result<Company> CreateCompany(Guid? id = null) =>
        Company.Create(
            id ?? Guid.NewGuid(),
            " Nexo Transport ",
            " bo-123-abc ",
            " Bolivia ",
            " La Paz ",
            " +59170000000 ",
            " CONTACTO@NEXO.TEST ",
            Now);
}
