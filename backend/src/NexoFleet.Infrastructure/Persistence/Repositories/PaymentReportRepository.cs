using Microsoft.EntityFrameworkCore;
using NexoFleet.Application.Abstractions.Persistence;
using NexoFleet.Domain.Payments;

namespace NexoFleet.Infrastructure.Persistence.Repositories;

internal sealed class PaymentReportRepository(ApplicationDbContext dbContext) : IPaymentReportRepository
{
    public Task<PaymentReport?> GetByIdAsync(
        Guid companyId,
        Guid id,
        CancellationToken cancellationToken = default) => Query()
        .SingleOrDefaultAsync(
            report => report.CompanyId == companyId && report.Id == id,
            cancellationToken);

    public Task<PaymentReport?> GetByPeriodAndEmployeeAsync(
        Guid companyId,
        Guid periodId,
        Guid employeeId,
        CancellationToken cancellationToken = default) => Query()
        .SingleOrDefaultAsync(
            report =>
                report.CompanyId == companyId &&
                report.PaymentPeriodId == periodId &&
                report.EmployeeId == employeeId,
            cancellationToken);

    public async Task<IReadOnlyList<PaymentReport>> ListByCompanyIdAsync(
        Guid companyId,
        CancellationToken cancellationToken = default) =>
        await Query()
            .Where(report => report.CompanyId == companyId)
            .OrderByDescending(report => report.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<PaymentReport>> ListByPeriodIdAsync(
        Guid companyId,
        Guid periodId,
        CancellationToken cancellationToken = default) =>
        await Query()
            .Where(report => report.CompanyId == companyId && report.PaymentPeriodId == periodId)
            .OrderByDescending(report => report.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public void Add(PaymentReport report) => dbContext.PaymentReports.Add(report);

    private IQueryable<PaymentReport> Query() => dbContext.PaymentReports
        .Include(report => report.Items)
        .Include(report => report.Comments)
        .Include(report => report.Files);
}
