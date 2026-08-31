using Microsoft.EntityFrameworkCore;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Domain.Payments;

namespace NexoFleet.Infrastructure.Persistence.Repositories;

internal sealed class PaymentPeriodRepository(ApplicationDbContext dbContext) : IPaymentPeriodRepository
{
    public Task<PaymentPeriod?> GetByIdAsync(
        Guid companyId,
        Guid id,
        CancellationToken cancellationToken = default) =>
        dbContext.PaymentPeriods.SingleOrDefaultAsync(
            period => period.CompanyId == companyId && period.Id == id,
            cancellationToken);

    public void Add(PaymentPeriod period) => dbContext.PaymentPeriods.Add(period);
}
