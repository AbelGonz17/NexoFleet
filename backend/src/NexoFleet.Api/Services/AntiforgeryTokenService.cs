using Microsoft.AspNetCore.Antiforgery;
using NexoFleet.Api.Dtos.Authentication;

namespace NexoFleet.Api.Services;

public sealed class AntiforgeryTokenService(IAntiforgery antiforgery)
{
    public CsrfTokenResponse IssueToken(HttpContext httpContext)
    {
        var tokens = antiforgery.GetAndStoreTokens(httpContext);
        return new CsrfTokenResponse(tokens.RequestToken ?? string.Empty);
    }
}
