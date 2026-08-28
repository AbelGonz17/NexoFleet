namespace NexoFleet.Domain.Payments;

public interface IPaymentPeriodRepository
{
    Task<PaymentPeriod?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(PaymentPeriod period);
}
