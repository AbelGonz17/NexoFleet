using NexoFleet.Domain.Payments;
using NexoFleet.Domain.Payments.Events;

namespace NexoFleet.Domain.UnitTests.Payments;

public sealed class PaymentTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 28, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void PeriodShouldValidateDatesAndControlStatus()
    {
        var invalid = PaymentPeriod.Create(Guid.NewGuid(), Guid.NewGuid(), "2026-08", new DateOnly(2026, 8, 31), new DateOnly(2026, 8, 1), Now);
        var period = CreatePeriod();

        Assert.Equal(PaymentErrors.InvalidPeriod, invalid.Error);
        Assert.True(period.Contains(new DateOnly(2026, 8, 15)));
        Assert.True(period.Close(Now.AddHours(1)).IsSuccess);
        Assert.Equal(PaymentPeriodStatus.Closed, period.Status);
        Assert.True(period.Reopen(Now.AddHours(2)).IsSuccess);
    }

    [Fact]
    public void ReportShouldCalculateBonusesAndDeductions()
    {
        var report = CreateReport();

        report.AddItem(Guid.NewGuid(), null, PaymentItemEffect.Addition, "Bonus", 100, Now.AddMinutes(1));
        report.AddItem(Guid.NewGuid(), null, PaymentItemEffect.Deduction, "Insurance", 50, Now.AddMinutes(2));

        Assert.Equal(100, report.Additions);
        Assert.Equal(50, report.Deductions);
        Assert.Equal(1050, report.TotalAmount);
    }

    [Fact]
    public void ItemCanOnlyBeChangedWhileDraft()
    {
        var report = CreateReport();
        var itemId = Guid.NewGuid();
        report.AddItem(itemId, null, PaymentItemEffect.Addition, "Bonus", 100, Now.AddMinutes(1));
        report.AddFile(Guid.NewGuid(), "report.pdf", "payments/report.pdf", "application/pdf", 1000, Guid.NewGuid(), Now.AddMinutes(2));
        report.Publish(Now.AddMinutes(3));

        var result = report.UpdateItem(itemId, PaymentItemEffect.Addition, "Bonus", 200, Now.AddMinutes(4));

        Assert.Equal(PaymentErrors.DraftRequired, result.Error);
    }

    [Fact]
    public void PublishShouldRequireUploadedReport()
    {
        var report = CreateReport();

        var result = report.Publish(Now.AddMinutes(1));

        Assert.Equal(PaymentErrors.FileRequiredToPublish, result.Error);
        Assert.Equal(PaymentReportStatus.Draft, report.Status);
    }

    [Fact]
    public void PublishShouldExposeFinalAmountAndRaiseEvent()
    {
        var report = CreateReport();
        report.AddItem(Guid.NewGuid(), null, PaymentItemEffect.Deduction, "Discount", 25, Now.AddMinutes(1));
        report.AddFile(Guid.NewGuid(), "report.pdf", "payments/report.pdf", "application/pdf", 1000, Guid.NewGuid(), Now.AddMinutes(2));

        var result = report.Publish(Now.AddMinutes(3));

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentReportStatus.Published, report.Status);
        var domainEvent = Assert.IsType<PaymentReportPublishedDomainEvent>(report.DomainEvents.Single());
        Assert.Equal(975, domainEvent.TotalAmount);
    }

    [Fact]
    public void VoidedReportShouldBeFinal()
    {
        var report = CreateReport();
        report.Void("Incorrect period", Now.AddMinutes(1));

        var result = report.AddComment(Guid.NewGuid(), Guid.NewGuid(), "Comment", Now.AddMinutes(2));

        Assert.Equal(PaymentErrors.VoidedStatusIsFinal, result.Error);
    }

    private static PaymentPeriod CreatePeriod() => PaymentPeriod.Create(
        Guid.NewGuid(), Guid.NewGuid(), " 2026-08 ", new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31), Now).Value;

    private static PaymentReport CreateReport() => PaymentReport.Create(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1000, "bob", Now).Value;
}
