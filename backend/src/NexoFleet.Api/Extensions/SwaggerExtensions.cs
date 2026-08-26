using System.Reflection;
using Microsoft.OpenApi;

namespace NexoFleet.Api.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "NexoFleet API",
                Version = "v1",
                Description =
                    "API multitenant para la gestión de empresas de transporte, empleados, rutas, viajes y pagos. " +
                    "Para probar operaciones protegidas desde Swagger: solicita primero el token CSRF, " +
                    "cópialo al encabezado X-XSRF-TOKEN del login y conserva las cookies generadas por el navegador."
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
            options.SupportNonNullableReferenceTypes();
            options.UseInlineDefinitionsForEnums();
        });

        return services;
    }

    public static WebApplication UseApiDocumentation(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "NexoFleet API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "NexoFleet API";
            options.DisplayRequestDuration();
            options.EnableTryItOutByDefault();
        });

        return app;
    }
}
