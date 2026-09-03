using Microsoft.AspNetCore.DataProtection;
using NexoFleet.Application;
using NexoFleet.Api.Extensions;
using NexoFleet.Api.Services;
using NexoFleet.Infrastructure;
using NexoFleet.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Data Protection keys persistence (avoid container restart key loss warning)
var keysPath = builder.Configuration["Storage:DataProtectionPath"]
    ?? Path.Combine(builder.Configuration["Storage:LocalPath"] ?? "uploads", "keys");
Directory.CreateDirectory(keysPath);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("NexoFleet");

builder.Services.AddControllers();
builder.Services.AddApiDocumentation();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = "NexoFleet.Xsrf";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});
builder.Services.AddScoped<AntiforgeryTokenService>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(
                builder.Configuration["Frontend:Origin"] ?? "http://localhost:3000",
                "http://localhost:5173",
                "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

await app.Services.SeedIdentityAsync(app.Configuration);

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("EnableSwagger", true))
{
    app.UseApiDocumentation();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler();
}

if (builder.Configuration.GetValue<bool>("UseHttpsRedirection", false))
{
    app.UseHttpsRedirection();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
