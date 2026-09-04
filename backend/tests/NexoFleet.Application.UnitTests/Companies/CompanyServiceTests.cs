using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Application.Authentication;
using NexoFleet.Application.Companies;
using NexoFleet.Application.Companies.Dtos;
using NexoFleet.Application.Companies.Validators;
using NexoFleet.Application.UnitTests.Fakes;
using NexoFleet.Domain.Common;
using NexoFleet.Domain.Companies;

namespace NexoFleet.Application.UnitTests.Companies;

public sealed class CompanyServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateAsyncWithValidRequestShouldCreateAndSaveCompany()
    {
        var repo = new FakeCompanyRepository();
        var uow = new FakeUnitOfWork();
        var clock = new FakeClock(Now);
        var service = CreateService(repo, uow, clock);

        var request = new CreateCompanyRequest(
            "TransExpress Corp",
            "J-12345678-9",
            "Venezuela",
            "Caracas",
            "+584121234567",
            "contact@transexpress.test");

        var result = await service.CreateAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal("TransExpress Corp", result.Value.Name);
        Assert.Equal("J-12345678-9", result.Value.TaxIdentification);
        Assert.Equal(CompanyStatus.Active.ToString(), result.Value.Status);
        Assert.Single(repo.Companies);
        Assert.Equal(1, uow.SaveChangesCalls);
    }

    [Fact]
    public async Task CreateAsyncWithDuplicateTaxIdShouldReturnConflictError()
    {
        var repo = new FakeCompanyRepository();
        var uow = new FakeUnitOfWork();
        var clock = new FakeClock(Now);
        var service = CreateService(repo, uow, clock);

        var existingCompany = Company.Create(
            Guid.NewGuid(),
            CompanyName.Create("Existing Corp").Value,
            TaxIdentification.Create("J-12345678-9").Value,
            Address.Create("Venezuela", "Caracas").Value,
            PhoneNumber.Create("+584121234567").Value,
            Email.Create("info@existing.test").Value,
            Now).Value;

        repo.Companies.Add(existingCompany);

        var request = new CreateCompanyRequest(
            "New Corp",
            "J-12345678-9",
            "Venezuela",
            "Caracas",
            "+584129999999",
            "new@corp.test");

        var result = await service.CreateAsync(request);

        Assert.True(result.IsFailure);
        Assert.Equal(CompanyErrors.TaxIdentificationDuplicate, result.Error);
        Assert.Equal(0, uow.SaveChangesCalls);
    }

    [Fact]
    public async Task CreateAsyncWithInvalidEmailShouldReturnValidationError()
    {
        var repo = new FakeCompanyRepository();
        var service = CreateService(repo);

        var request = new CreateCompanyRequest(
            "Invalid Corp",
            "J-12345678-9",
            "Venezuela",
            "Caracas",
            "+584121234567",
            "not-an-email");

        var result = await service.CreateAsync(request);

        Assert.True(result.IsFailure);
        Assert.IsType<ValidationError>(result.Error);
    }

    [Fact]
    public async Task UpdateProfileAsyncWhenCompanyExistsShouldUpdateFields()
    {
        var repo = new FakeCompanyRepository();
        var uow = new FakeUnitOfWork();
        var clock = new FakeClock(Now);
        var service = CreateService(repo, uow, clock);

        var company = Company.Create(
            Guid.NewGuid(),
            CompanyName.Create("Old Name").Value,
            TaxIdentification.Create("J-11111111-1").Value,
            Address.Create("Venezuela", "Caracas").Value,
            PhoneNumber.Create("+584121111111").Value,
            Email.Create("old@test.com").Value,
            Now).Value;

        repo.Companies.Add(company);

        var updateRequest = new UpdateCompanyProfileRequest(
            "New Name",
            "J-22222222-2",
            "Venezuela",
            "Valencia",
            "+584122222222",
            "new@test.com");

        var result = await service.UpdateProfileAsync(company.Id, updateRequest);

        Assert.True(result.IsSuccess);
        Assert.Equal("New Name", result.Value.Name);
        Assert.Equal("J-22222222-2", result.Value.TaxIdentification);
        Assert.Equal("Valencia", result.Value.City);
        Assert.Equal(1, uow.SaveChangesCalls);
    }

    [Fact]
    public async Task SuspendAndActivateShouldTransitionStatus()
    {
        var repo = new FakeCompanyRepository();
        var uow = new FakeUnitOfWork();
        var clock = new FakeClock(Now);
        var service = CreateService(repo, uow, clock);

        var company = Company.Create(
            Guid.NewGuid(),
            CompanyName.Create("Status Corp").Value,
            TaxIdentification.Create("J-99999999-9").Value,
            Address.Create("Venezuela", "Caracas").Value,
            PhoneNumber.Create("+584121111111").Value,
            Email.Create("status@test.com").Value,
            Now).Value;

        repo.Companies.Add(company);

        var suspendResult = await service.SuspendAsync(company.Id);
        Assert.True(suspendResult.IsSuccess);
        Assert.Equal(CompanyStatus.Suspended, company.Status);

        var activateResult = await service.ActivateAsync(company.Id);
        Assert.True(activateResult.IsSuccess);
        Assert.Equal(CompanyStatus.Active, company.Status);
    }

    [Fact]
    public async Task CreateAdminAsyncWithValidRequestShouldCreateUser()
    {
        var repo = new FakeCompanyRepository();
        var fakeIdentity = new FakeIdentityService();
        var service = CreateService(repo, identity: fakeIdentity);

        var company = Company.Create(
            Guid.NewGuid(),
            CompanyName.Create("Test Corp").Value,
            TaxIdentification.Create("J-12345678-9").Value,
            Address.Create("Venezuela", "Caracas").Value,
            PhoneNumber.Create("+584121111111").Value,
            Email.Create("corp@test.com").Value,
            Now).Value;

        repo.Companies.Add(company);

        var request = new CreateCompanyAdminRequest(
            "Carlos",
            "Mendoza",
            "carlos@testcorp.test",
            "SecurePass123!");

        var result = await service.CreateAdminAsync(company.Id, request);

        Assert.True(result.IsSuccess);
        Assert.Equal("carlos@testcorp.test", result.Value.Email);
        Assert.Equal(company.Id, result.Value.CompanyId);
        Assert.Contains("Administrator", result.Value.Roles);
    }

    private static CompanyService CreateService(
        FakeCompanyRepository repo,
        FakeUnitOfWork? uow = null,
        FakeClock? clock = null,
        FakeIdentityService? identity = null)
    {
        return new CompanyService(
            repo,
            identity ?? new FakeIdentityService(),
            uow ?? new FakeUnitOfWork(),
            clock ?? new FakeClock(Now),
            new CreateCompanyRequestValidator(),
            new UpdateCompanyProfileRequestValidator(),
            new CreateCompanyAdminRequestValidator());
    }

    private sealed class FakeIdentityService : NexoFleet.Application.Abstractions.Authentication.IIdentityService
    {
        public Task<Result<AuthenticatedUser>> PasswordSignInAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<AuthenticatedUser>.Failure(NexoFleet.Application.Authentication.AuthErrors.InvalidCredentials));

        public Task<AuthenticatedUser?> GetUserAsync(
            Guid userId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<AuthenticatedUser?>(null);

        public Task<Result<AuthenticatedUser>> CreateUserAsync(
            string email,
            string password,
            string firstName,
            string lastName,
            Guid? companyId,
            string role,
            CancellationToken cancellationToken = default)
        {
            var user = new AuthenticatedUser(
                Guid.NewGuid(),
                email,
                firstName,
                lastName,
                companyId,
                "Test Company",
                [role]);
            return Task.FromResult(Result<AuthenticatedUser>.Success(user));
        }

        public Task<IReadOnlyList<AuthenticatedUser>> GetUsersByCompanyIdAsync(
            Guid companyId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<AuthenticatedUser>>(Array.Empty<AuthenticatedUser>());

        public Task SignOutAsync() => Task.CompletedTask;
    }
}
