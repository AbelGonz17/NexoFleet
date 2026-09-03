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

    public Task<bool> ExistsByCodeAsync(
        Guid companyId,
        string code,
        Guid? excludingPeriodId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return dbContext.PaymentPeriods.AnyAsync(
            period => period.CompanyId == companyId &&
                      period.Code == normalized &&
                      (!excludingPeriodId.HasValue || period.Id != excludingPeriodId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentPeriod>> ListByCompanyIdAsync(
        Guid companyId,
        CancellationToken cancellationToken = default) =>
        await dbContext.PaymentPeriods
            .Where(period => period.CompanyId == companyId)
            .OrderByDescending(period => period.StartsOn)
            .ToListAsync(cancellationToken);

    public void Add(PaymentPeriod period) => dbContext.PaymentPeriods.Add(period);
}
