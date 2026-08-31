using NexoFleet.Domain.Payments;

namespace NexoFleet.Application.Abstractions.Persistence;

public interface IPaymentPeriodRepository
{
    Task<PaymentPeriod?> GetByIdAsync(
        Guid companyId,
        Guid id,
        CancellationToken cancellationToken = default);

    void Add(PaymentPeriod period);
}
