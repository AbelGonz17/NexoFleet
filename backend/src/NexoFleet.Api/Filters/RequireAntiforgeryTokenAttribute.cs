using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NexoFleet.Api.Common;
using NexoFleet.Api.Extensions;

namespace NexoFleet.Api.Filters;

[AttributeUsage(AttributeTargets.Method)]
public sealed class RequireAntiforgeryTokenAttribute()
    : TypeFilterAttribute(typeof(AntiforgeryValidationFilter));

internal sealed class AntiforgeryValidationFilter(IAntiforgery antiforgery)
    : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(context.HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            context.Result = context.HttpContext.ToErrorResult(
                ApiErrors.InvalidAntiforgeryToken);
        }
    }
}
