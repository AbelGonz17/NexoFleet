using NexoFleet.Application.Payments;
using NexoFleet.Application.Payments.Dtos;
using NexoFleet.Application.Payments.Validators;
using NexoFleet.Application.UnitTests.Fakes;
using NexoFleet.Domain.Payments;

namespace NexoFleet.Application.UnitTests.Payments;

public sealed class PaymentPeriodServiceTests
{
    private static readonly Guid CompanyId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2026, 3, 1, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateAsyncShouldCreatePaymentPeriodWhenValid()
    {
        var repo = new FakePaymentPeriodRepository();
        var tenant = new FakeCurrentTenant(CompanyId);
        var uow = new FakeUnitOfWork();
        var clock = new FakeClock(Now);
        var service = new PaymentPeriodService(repo, tenant, uow, clock, new CreatePaymentPeriodRequestValidator());

        var request = new CreatePaymentPeriodRequest("2026-Q1-01", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 15));
        var result = await service.CreateAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal("2026-Q1-01", result.Value.Code);
        Assert.Equal(PaymentPeriodStatus.Open.ToString(), result.Value.Status);
        Assert.Single(repo.Periods);
        Assert.Equal(1, uow.SaveChangesCalls);
    }

    [Fact]
    public async Task CloseAndReopenShouldTransitionStatus()
    {
        var repo = new FakePaymentPeriodRepository();
        var tenant = new FakeCurrentTenant(CompanyId);
        var uow = new FakeUnitOfWork();
        var clock = new FakeClock(Now);
        var service = new PaymentPeriodService(repo, tenant, uow, clock, new CreatePaymentPeriodRequestValidator());

        var period = PaymentPeriod.Create(
            Guid.NewGuid(),
            CompanyId,
            "2026-Q1-02",
            new DateOnly(2026, 3, 16),
            new DateOnly(2026, 3, 31),
            Now).Value;
        repo.Periods.Add(period);

        var closeResult = await service.CloseAsync(period.Id);
        Assert.True(closeResult.IsSuccess);
        Assert.Equal(PaymentPeriodStatus.Closed, period.Status);

        var reopenResult = await service.ReopenAsync(period.Id);
        Assert.True(reopenResult.IsSuccess);
        Assert.Equal(PaymentPeriodStatus.Open, period.Status);
    }
}
