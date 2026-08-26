using NexoFleet.Application;
using NexoFleet.Api.Extensions;
using NexoFleet.Api.Services;
using NexoFleet.Infrastructure;
using NexoFleet.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApiDocumentation();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = "NexoFleet.Xsrf";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});
builder.Services.AddScoped<AntiforgeryTokenService>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(builder.Configuration["Frontend:Origin"] ?? "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

await app.Services.SeedIdentityAsync(app.Configuration);

if (app.Environment.IsDevelopment())
{
    app.UseApiDocumentation();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
