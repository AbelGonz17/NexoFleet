using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NexoFleet.Application.Abstractions.Context;

namespace NexoFleet.Infrastructure.Identity;

internal sealed class CurrentTenant(IHttpContextAccessor httpContextAccessor) : ICurrentTenant
{
    public Guid? CompanyId => Guid.TryParse(
        httpContextAccessor.HttpContext?.User.FindFirstValue(CustomClaimTypes.CompanyId),
        out var companyId)
            ? companyId
            : null;

    public bool IsAvailable => CompanyId.HasValue;
}
