using NexoFleet.Domain.Payments;

namespace NexoFleet.Application.Abstractions.Persistence;

public interface IPaymentReportRepository
{
    Task<PaymentReport?> GetByIdAsync(
        Guid companyId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PaymentReport?> GetByPeriodAndEmployeeAsync(
        Guid companyId,
        Guid periodId,
        Guid employeeId,
        CancellationToken cancellationToken = default);

    void Add(PaymentReport report);
}
