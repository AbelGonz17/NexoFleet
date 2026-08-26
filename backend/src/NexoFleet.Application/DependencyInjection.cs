using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NexoFleet.Application.Authentication;

namespace NexoFleet.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));
        services.AddScoped<AuthService>();

        return services;
    }
}
