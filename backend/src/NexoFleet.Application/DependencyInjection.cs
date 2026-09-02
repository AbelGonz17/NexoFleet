using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NexoFleet.Application.Authentication;
using NexoFleet.Application.Clients;
using NexoFleet.Application.Companies;
using NexoFleet.Application.Employees;
using NexoFleet.Application.Routes;
using NexoFleet.Application.RouteSchedules;
using NexoFleet.Application.Trips;
using NexoFleet.Application.Vehicles;

namespace NexoFleet.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));
        services.AddScoped<AuthService>();
        services.AddScoped<CompanyService>();
        services.AddScoped<ClientService>();
        services.AddScoped<EmployeeService>();
        services.AddScoped<VehicleService>();
        services.AddScoped<RouteService>();
        services.AddScoped<RouteScheduleService>();
        services.AddScoped<TripService>();

        return services;
    }
}
