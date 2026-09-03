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

    Task<IReadOnlyList<PaymentReport>> ListByCompanyIdAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentReport>> ListByPeriodIdAsync(
        Guid companyId,
        Guid periodId,
        CancellationToken cancellationToken = default);

    void Add(PaymentReport report);
}
