using NexoFleet.Domain.Clients;
using NexoFleet.Domain.Clients.Events;
using NexoFleet.Domain.Common;
using NexoFleet.Domain.Companies;

namespace NexoFleet.Domain.UnitTests.Clients;

public sealed class ClientTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 28, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateShouldCreateClientAndRaiseEvent()
    {
        var id = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var result = CreateClient(id, companyId);

        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value.Id);
        Assert.Equal(companyId, result.Value.CompanyId);
        Assert.Equal("CLI-001", result.Value.ClientCode.Value);
        Assert.Equal("Acme Logistics", result.Value.Name.Value);
        Assert.Equal("NIT-123", result.Value.TaxIdentification?.Value);
        Assert.Equal("Jane Doe", result.Value.ContactName?.Value);
        Assert.Equal("+59170000001", result.Value.Phone?.Value);
        Assert.Equal("contact@acme.test", result.Value.Email?.Value);
        Assert.Equal(ClientStatus.Active, result.Value.Status);
        Assert.IsType<ClientCreatedDomainEvent>(result.Value.DomainEvents.Single());
    }

    [Fact]
    public void CreateShouldFailWhenIdIsEmpty()
    {
        var code = ClientCode.Create("CLI-1").Value;
        var name = ClientName.Create("Client").Value;

        var result = Client.Create(Guid.Empty, Guid.NewGuid(), code, name, null, null, null, null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal(ClientErrors.InvalidId, result.Error);
    }

    [Fact]
    public void CreateShouldFailWhenCompanyIdIsEmpty()
    {
        var code = ClientCode.Create("CLI-1").Value;
        var name = ClientName.Create("Client").Value;

        var result = Client.Create(Guid.NewGuid(), Guid.Empty, code, name, null, null, null, null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal(ClientErrors.InvalidCompanyId, result.Error);
    }

    [Fact]
    public void UpdateShouldBeIdempotent()
    {
        var client = CreateClient().Value;
        client.ClearDomainEvents();

        var result = client.UpdateProfile(
            client.ClientCode,
            client.Name,
            client.TaxIdentification,
            client.ContactName,
            client.Phone,
            client.Email,
            Now.AddHours(1));

        Assert.True(result.IsSuccess);
        Assert.Null(client.UpdatedAtUtc);
    }

    [Fact]
    public void DeactivateAndActivateShouldControlStatus()
    {
        var client = CreateClient().Value;

        Assert.True(client.Deactivate(Now.AddHours(1)).IsSuccess);
        Assert.Equal(ClientStatus.Inactive, client.Status);
        Assert.True(client.Activate(Now.AddHours(2)).IsSuccess);
        Assert.Equal(ClientStatus.Active, client.Status);
    }

    private static Result<Client> CreateClient(Guid? id = null, Guid? companyId = null)
    {
        var code = ClientCode.Create(" cli-001 ").Value;
        var name = ClientName.Create(" Acme Logistics ").Value;
        var taxId = TaxIdentification.Create(" nit-123 ").Value;
        var contact = ContactName.Create(" Jane Doe ").Value;
        var phone = PhoneNumber.Create(" +59170000001 ").Value;
        var email = Email.Create(" CONTACT@ACME.TEST ").Value;

        return Client.Create(
            id ?? Guid.NewGuid(),
            companyId ?? Guid.NewGuid(),
            code,
            name,
            taxId,
            contact,
            phone,
            email,
            Now);
    }
}
