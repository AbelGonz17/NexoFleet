using NexoFleet.Domain.Common;
using NexoFleet.Domain.Companies;
using NexoFleet.Domain.Companies.Events;

namespace NexoFleet.Domain.UnitTests.Companies;

public sealed class CompanyTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateShouldCreateCompanyAndRaiseDomainEvent()
    {
        var id = Guid.NewGuid();

        var result = CreateCompany(id);

        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value.Id);
        Assert.Equal("Nexo Transport", result.Value.Name.Value);
        Assert.Equal("BO-123-ABC", result.Value.TaxIdentification.Value);
        Assert.Equal("Bolivia", result.Value.Address.Country);
        Assert.Equal("La Paz", result.Value.Address.City);
        Assert.Equal("+59170000000", result.Value.Phone.Value);
        Assert.Equal("contacto@nexo.test", result.Value.Email.Value);
        Assert.Equal(CompanyStatus.Active, result.Value.Status);
        Assert.Equal(Now, result.Value.CreatedAtUtc);
        Assert.Single(result.Value.DomainEvents);
        Assert.IsType<CompanyCreatedDomainEvent>(result.Value.DomainEvents.Single());
    }

    [Fact]
    public void CreateShouldFailWhenIdIsEmpty()
    {
        var name = CompanyName.Create("Nexo Transport").Value;
        var taxId = TaxIdentification.Create("BO-123-ABC").Value;
        var address = Address.Create("Bolivia", "La Paz").Value;
        var phone = PhoneNumber.Create("+59170000000").Value;
        var email = Email.Create("contacto@nexo.test").Value;

        var result = Company.Create(
            Guid.Empty,
            name,
            taxId,
            address,
            phone,
            email,
            Now);

        Assert.True(result.IsFailure);
        Assert.Equal(CompanyErrors.InvalidId, result.Error);
    }

    [Fact]
    public void UpdateProfileShouldChangeDataAndRaiseDomainEvent()
    {
        var company = CreateCompany().Value;
        company.ClearDomainEvents();

        var newName = CompanyName.Create("Nexo Fleet SRL").Value;
        var newTaxId = TaxIdentification.Create("bo-456").Value;
        var newAddress = Address.Create("Bolivia", "Santa Cruz").Value;
        var newPhone = PhoneNumber.Create("+59171111111").Value;
        var newEmail = Email.Create("ADMIN@NEXO.TEST").Value;

        var result = company.UpdateProfile(
            newName,
            newTaxId,
            newAddress,
            newPhone,
            newEmail,
            Now.AddHours(1));

        Assert.True(result.IsSuccess);
        Assert.Equal("Nexo Fleet SRL", company.Name.Value);
        Assert.Equal("BO-456", company.TaxIdentification.Value);
        Assert.Equal("Santa Cruz", company.Address.City);
        Assert.Equal("+59171111111", company.Phone.Value);
        Assert.Equal("admin@nexo.test", company.Email.Value);
        Assert.Equal(Now.AddHours(1), company.UpdatedAtUtc);
        Assert.Single(company.DomainEvents);
        Assert.IsType<CompanyProfileUpdatedDomainEvent>(company.DomainEvents.Single());
    }

    [Fact]
    public void UpdateProfileShouldNotRaiseEventWhenNothingChanged()
    {
        var company = CreateCompany().Value;
        company.ClearDomainEvents();

        var sameName = CompanyName.Create("Nexo Transport").Value;
        var sameTaxId = TaxIdentification.Create("BO-123-ABC").Value;
        var sameAddress = Address.Create("Bolivia", "La Paz").Value;
        var samePhone = PhoneNumber.Create("+59170000000").Value;
        var sameEmail = Email.Create("contacto@nexo.test").Value;

        var result = company.UpdateProfile(
            sameName,
            sameTaxId,
            sameAddress,
            samePhone,
            sameEmail,
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

    [Fact]
    public void ActivateShouldFailWhenCompanyIsAlreadyActive()
    {
        var company = CreateCompany().Value;

        var result = company.Activate(Now.AddHours(1));

        Assert.True(result.IsFailure);
        Assert.Equal(CompanyErrors.AlreadyActive, result.Error);
    }

    private static Result<Company> CreateCompany(Guid? id = null)
    {
        var name = CompanyName.Create(" Nexo Transport ").Value;
        var taxId = TaxIdentification.Create(" bo-123-abc ").Value;
        var address = Address.Create(" Bolivia ", " La Paz ").Value;
        var phone = PhoneNumber.Create(" +59170000000 ").Value;
        var email = Email.Create(" CONTACTO@NEXO.TEST ").Value;

        return Company.Create(
            id ?? Guid.NewGuid(),
            name,
            taxId,
            address,
            phone,
            email,
            Now);
    }
}
