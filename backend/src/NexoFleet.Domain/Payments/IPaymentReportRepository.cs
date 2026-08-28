namespace NexoFleet.Domain.Payments;

public interface IPaymentReportRepository
{
    Task<PaymentReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaymentReport?> GetByPeriodAndEmployeeAsync(Guid periodId, Guid employeeId, CancellationToken cancellationToken = default);
    void Add(PaymentReport report);
}
