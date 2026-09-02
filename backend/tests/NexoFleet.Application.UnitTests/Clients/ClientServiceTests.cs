using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Application.Clients;
using NexoFleet.Application.Clients.Dtos;
using NexoFleet.Application.Clients.Validators;
using NexoFleet.Application.UnitTests.Fakes;
using NexoFleet.Domain.Clients;
using NexoFleet.Domain.Common;

namespace NexoFleet.Application.UnitTests.Clients;

public sealed class ClientServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid CompanyId = Guid.NewGuid();

    [Fact]
    public async Task CreateAsyncWithValidRequestShouldCreateClient()
    {
        var repo = new FakeClientRepository();
        var uow = new FakeUnitOfWork();
        var tenant = new FakeCurrentTenant(CompanyId);
        var clock = new FakeClock(Now);
        var service = CreateService(repo, tenant, uow, clock);

        var request = new CreateClientRequest(
            "CLI-001",
            "Banesco Banco Universal",
            "J-07013380-5",
            "Carlos Perez",
            "+584141234567",
            "contacto@banesco.test");

        var result = await service.CreateAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal("CLI-001", result.Value.ClientCode);
        Assert.Equal("Banesco Banco Universal", result.Value.Name);
        Assert.Equal(CompanyId, result.Value.CompanyId);
        Assert.Equal(ClientStatus.Active.ToString(), result.Value.Status);
        Assert.Single(repo.Clients);
        Assert.Equal(1, uow.SaveChangesCalls);
    }

    [Fact]
    public async Task CreateAsyncWhenTenantNotSetShouldReturnInvalidCompanyId()
    {
        var repo = new FakeClientRepository();
        var tenant = new FakeCurrentTenant(null);
        var service = CreateService(repo, tenant);

        var request = new CreateClientRequest("CLI-001", "Some Client");
        var result = await service.CreateAsync(request);

        Assert.True(result.IsFailure);
        Assert.Equal(ClientErrors.InvalidCompanyId, result.Error);
    }

    [Fact]
    public async Task CreateAsyncWithDuplicateCodeShouldReturnConflictError()
    {
        var repo = new FakeClientRepository();
        var tenant = new FakeCurrentTenant(CompanyId);
        var service = CreateService(repo, tenant);

        var existingClient = Client.Create(
            Guid.NewGuid(),
            CompanyId,
            ClientCode.Create("CLI-001").Value,
            ClientName.Create("Existing Client").Value,
            null, null, null, null,
            Now).Value;

        repo.Clients.Add(existingClient);

        var request = new CreateClientRequest("CLI-001", "Another Name");
        var result = await service.CreateAsync(request);

        Assert.True(result.IsFailure);
        Assert.Equal(ClientErrors.ClientCodeDuplicate, result.Error);
    }

    [Fact]
    public async Task DeactivateAndActivateShouldChangeStatus()
    {
        var repo = new FakeClientRepository();
        var uow = new FakeUnitOfWork();
        var tenant = new FakeCurrentTenant(CompanyId);
        var service = CreateService(repo, tenant, uow);

        var client = Client.Create(
            Guid.NewGuid(),
            CompanyId,
            ClientCode.Create("CLI-001").Value,
            ClientName.Create("Client 1").Value,
            null, null, null, null,
            Now).Value;

        repo.Clients.Add(client);

        var deactivateResult = await service.DeactivateAsync(client.Id);
        Assert.True(deactivateResult.IsSuccess);
        Assert.Equal(ClientStatus.Inactive, client.Status);

        var activateResult = await service.ActivateAsync(client.Id);
        Assert.True(activateResult.IsSuccess);
        Assert.Equal(ClientStatus.Active, client.Status);
    }

    private static ClientService CreateService(
        FakeClientRepository repo,
        FakeCurrentTenant? tenant = null,
        FakeUnitOfWork? uow = null,
        FakeClock? clock = null)
    {
        return new ClientService(
            repo,
            tenant ?? new FakeCurrentTenant(CompanyId),
            uow ?? new FakeUnitOfWork(),
            clock ?? new FakeClock(Now),
            new CreateClientRequestValidator(),
            new UpdateClientRequestValidator());
    }

    private sealed class FakeClientRepository : IClientRepository
    {
        public List<Client> Clients { get; } = [];

        public Task<Client?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Clients.SingleOrDefault(c => c.CompanyId == companyId && c.Id == id));

        public Task<bool> ExistsByCodeAsync(Guid companyId, string clientCode, Guid? excludingClientId = null, CancellationToken cancellationToken = default)
        {
            var normalized = clientCode.Trim().ToUpperInvariant();
            return Task.FromResult(Clients.Any(c =>
                c.CompanyId == companyId &&
                c.ClientCode.Value == normalized &&
                (!excludingClientId.HasValue || c.Id != excludingClientId.Value)));
        }

        public Task<IReadOnlyList<Client>> ListByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Client>>(Clients.Where(c => c.CompanyId == companyId).OrderBy(c => c.Name.Value).ToArray());

        public void Add(Client client) => Clients.Add(client);
    }
}
