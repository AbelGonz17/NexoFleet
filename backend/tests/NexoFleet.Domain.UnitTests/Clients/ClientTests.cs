using NexoFleet.Domain.Clients;
using NexoFleet.Domain.Clients.Events;

namespace NexoFleet.Domain.UnitTests.Clients;

public sealed class ClientTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 28, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateShouldNormalizeProfileAndRaiseEvent()
    {
        var result = CreateClient();

        Assert.True(result.IsSuccess);
        Assert.Equal("CLI-001", result.Value.ClientCode);
        Assert.Equal("Acme Logistics", result.Value.Name);
        Assert.Equal("NIT-123", result.Value.TaxIdentification);
        Assert.Equal("contact@acme.test", result.Value.Email);
        Assert.Equal(ClientStatus.Active, result.Value.Status);
        Assert.IsType<ClientCreatedDomainEvent>(result.Value.DomainEvents.Single());
    }

    [Fact]
    public void CreateShouldRejectInvalidEmail()
    {
        var result = Client.Create(Guid.NewGuid(), Guid.NewGuid(), "CLI-1", "Client", null, null, null, "invalid", Now);

        Assert.True(result.IsFailure);
        Assert.Equal(ClientErrors.EmailInvalid, result.Error);
    }

    [Fact]
    public void UpdateShouldBeIdempotent()
    {
        var client = CreateClient().Value;
        client.ClearDomainEvents();

        var result = client.UpdateProfile(client.ClientCode, client.Name, client.TaxIdentification, client.ContactName, client.Phone, client.Email, Now.AddHours(1));

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

    private static NexoFleet.Domain.Common.Result<Client> CreateClient() => Client.Create(
        Guid.NewGuid(), Guid.NewGuid(), " cli-001 ", " Acme Logistics ", " nit-123 ", " Jane Doe ", " +59170000001 ", " CONTACT@ACME.TEST ", Now);
}
