using NexoFleet.Application.Payments;
using NexoFleet.Application.Payments.Dtos;
using NexoFleet.Application.Payments.Validators;
using NexoFleet.Application.UnitTests.Fakes;
using NexoFleet.Domain.Common;
using NexoFleet.Domain.Employees;
using NexoFleet.Domain.Payments;

namespace NexoFleet.Application.UnitTests.Payments;

public sealed class PaymentReportServiceTests
{
    private static readonly Guid CompanyId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2026, 3, 1, 10, 0, 0, TimeSpan.Zero);

    private static PaymentReportService CreateService(
        FakePaymentReportRepository reportRepo,
        FakePaymentPeriodRepository periodRepo,
        FakeEmployeeRepository empRepo,
        FakeTripRepository tripRepo,
        FakeCurrentTenant tenant,
        FakeCurrentUser currentUser,
        FakeUnitOfWork uow,
        FakeClock clock)
    {
        return new PaymentReportService(
            reportRepo,
            periodRepo,
            empRepo,
            tripRepo,
            tenant,
            currentUser,
            uow,
            clock,
            new CreatePaymentReportRequestValidator(),
            new UpdatePaymentReportBaseAmountRequestValidator(),
            new AddPaymentItemRequestValidator(),
            new UpdatePaymentItemRequestValidator(),
            new AddPaymentCommentRequestValidator(),
            new AddPaymentReportFileRequestValidator(),
            new VoidPaymentReportRequestValidator());
    }

    [Fact]
    public async Task PaymentReportLifecycleShouldCreateAddItemsFilesAndPublish()
    {
        var reportRepo = new FakePaymentReportRepository();
        var periodRepo = new FakePaymentPeriodRepository();
        var empRepo = new FakeEmployeeRepository();
        var tripRepo = new FakeTripRepository();
        var tenant = new FakeCurrentTenant(CompanyId);
        var user = new FakeCurrentUser(UserId);
        var uow = new FakeUnitOfWork();
        var clock = new FakeClock(Now);
        var service = CreateService(reportRepo, periodRepo, empRepo, tripRepo, tenant, user, uow, clock);

        var period = PaymentPeriod.Create(
            Guid.NewGuid(),
            CompanyId,
            "PER-01",
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 15),
            Now).Value;
        periodRepo.Periods.Add(period);

        var employee = Employee.Create(
            Guid.NewGuid(),
            CompanyId,
            EmployeeCode.Create("EMP-001").Value,
            FullName.Create("Luis", "Navarro").Value,
            IdentityDocument.Create("V-18223344").Value,
            PhoneNumber.Create("+584141112233").Value,
            Email.Create("luis@test.com").Value,
            new DateOnly(2025, 1, 1),
            Now).Value;
        empRepo.Employees.Add(employee);

        // 1. Create Report
        var createRequest = new CreatePaymentReportRequest(period.Id, employee.Id, 500.00m, "USD");
        var createResult = await service.CreateAsync(createRequest);
        Assert.True(createResult.IsSuccess);
        Assert.Equal(500.00m, createResult.Value.BaseAmount);
        Assert.Equal(PaymentReportStatus.Draft.ToString(), createResult.Value.Status);

        var reportId = createResult.Value.Id;

        // 2. Add Addition Item (Bonificación)
        var addItemResult = await service.AddItemAsync(reportId, new AddPaymentItemRequest(
            PaymentItemEffect.Addition,
            "Bono puntualidad",
            50.00m));
        Assert.True(addItemResult.IsSuccess);
        Assert.Equal(550.00m, addItemResult.Value.TotalAmount);

        // 3. Add Deduction Item (Adelanto)
        var addDeductResult = await service.AddItemAsync(reportId, new AddPaymentItemRequest(
            PaymentItemEffect.Deduction,
            "Adelanto quincenal",
            100.00m));
        Assert.True(addDeductResult.IsSuccess);
        Assert.Equal(450.00m, addDeductResult.Value.TotalAmount);

        // 4. Add File
        var addFileResult = await service.AddFileAsync(reportId, new AddPaymentReportFileRequest(
            "recibo_pago.pdf",
            "payments/recibo_pago.pdf",
            "application/pdf",
            102400));
        Assert.True(addFileResult.IsSuccess);

        // 5. Add Comment
        var addCommentResult = await service.AddCommentAsync(reportId, new AddPaymentCommentRequest("Aprobado por administración"));
        Assert.True(addCommentResult.IsSuccess);

        // 6. Publish Report
        var publishResult = await service.PublishAsync(reportId);
        Assert.True(publishResult.IsSuccess);
        Assert.Equal(PaymentReportStatus.Published.ToString(), publishResult.Value.Status);
    }
}
