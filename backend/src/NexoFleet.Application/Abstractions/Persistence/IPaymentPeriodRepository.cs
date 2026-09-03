using NexoFleet.Domain.Payments;

namespace NexoFleet.Application.Abstractions.Persistence;

public interface IPaymentPeriodRepository
{
    Task<PaymentPeriod?> GetByIdAsync(
        Guid companyId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeAsync(
        Guid companyId,
        string code,
        Guid? excludingPeriodId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentPeriod>> ListByCompanyIdAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    void Add(PaymentPeriod period);
}
