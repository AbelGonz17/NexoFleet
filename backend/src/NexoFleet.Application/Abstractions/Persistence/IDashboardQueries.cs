using NexoFleet.Application.Dashboards.Dtos;

namespace NexoFleet.Application.Abstractions.Persistence;

public interface IDashboardQueries
{
    Task<IReadOnlyList<GlobalKpiResponse>> GetGlobalKpisAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TopCompanyResponse>> GetTopCompaniesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditFeedResponse>> GetAuditFeedAsync(CancellationToken cancellationToken = default);
    Task<CompanyStatsResponse> GetCompanyStatsAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecentTripResponse>> GetRecentTripsAsync(Guid companyId, CancellationToken cancellationToken = default);
}
