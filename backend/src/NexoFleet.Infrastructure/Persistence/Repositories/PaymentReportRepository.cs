using Microsoft.EntityFrameworkCore;
using NexoFleet.Domain.Payments;

namespace NexoFleet.Infrastructure.Persistence.Repositories;

internal sealed class PaymentReportRepository(ApplicationDbContext dbContext) : IPaymentReportRepository
{
    public Task<PaymentReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Query()
        .SingleOrDefaultAsync(report => report.Id == id, cancellationToken);

    public Task<PaymentReport?> GetByPeriodAndEmployeeAsync(Guid periodId, Guid employeeId, CancellationToken cancellationToken = default) => Query()
        .SingleOrDefaultAsync(report => report.PaymentPeriodId == periodId && report.EmployeeId == employeeId, cancellationToken);

    public void Add(PaymentReport report) => dbContext.PaymentReports.Add(report);

    private IQueryable<PaymentReport> Query() => dbContext.PaymentReports
        .Include(report => report.Items)
        .Include(report => report.Comments)
        .Include(report => report.Files);
}
