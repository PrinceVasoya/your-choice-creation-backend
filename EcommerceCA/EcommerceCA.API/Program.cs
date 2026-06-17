using EcommerceCA.API.Extensions;
using EcommerceCA.API.Middleware;
using EcommerceCA.Infrastructure.Data;
using EcommerceCA.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Serilog;

// ── Bootstrap logger ───────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    // Load .env file if it exists in current or parent directories
    try
    {
        var currentDir = System.IO.Directory.GetCurrentDirectory();
        var envPath = System.IO.Path.Combine(currentDir, ".env");
        if (!System.IO.File.Exists(envPath))
        {
            envPath = System.IO.Path.Combine(System.IO.Directory.GetParent(currentDir)?.FullName ?? "", ".env");
        }
        if (!System.IO.File.Exists(envPath))
        {
            envPath = System.IO.Path.Combine(System.IO.Directory.GetParent(System.IO.Directory.GetParent(currentDir)?.FullName ?? "")?.FullName ?? "", ".env");
        }
        if (!System.IO.File.Exists(envPath))
        {
            envPath = System.IO.Path.Combine(currentDir, "../ycc-server/.env");
        }
        if (!System.IO.File.Exists(envPath))
        {
            envPath = System.IO.Path.Combine(currentDir, "../ycc-client/.env");
        }

        if (System.IO.File.Exists(envPath))
        {
            foreach (var line in System.IO.File.ReadAllLines(envPath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                    continue;

                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim().Trim('"').Trim('\'');
                    System.Environment.SetEnvironmentVariable(key, value);
                }
            }
        }
    }
    catch { /* Ignore env load errors */ }

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ────────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console()
        .WriteTo.File("logs/ecommerce-.log", rollingInterval: RollingInterval.Day)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("MachineName", System.Environment.MachineName));

    var config = builder.Configuration;

    // ── Register all services ──────────────────────────────────────────────────
    builder.Services
        .AddDatabase(config)
        .AddIdentityServices()
        .AddJwtAuthentication(config)          // from API/Extensions
        .AddSwaggerWithJwt()                   // from API/Extensions
        .AddCorsPolicy(config)                 // from API/Extensions
        .AddApplicationLayer()                 // AutoMapper + FluentValidation
        .AddHelpers()                          // JwtHelper singleton
        .AddRepositories()                     // all repository registrations
        .AddApplicationServices()             // Auth, Category, Product, Cart, Order
        .AddInfrastructureServices()           // Payment, Image, WhatsApp, Email, User
        .AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

    var app = builder.Build();

    // ── Auto-migrate on startup ─────────────────────────────────────────────
    if (app.Environment.IsDevelopment())
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
            DbSeeder.SeedData(db);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to initialize and seed database. Proceeding with application startup.");
        }
    }
    else
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Failed to apply database migrations in Production.");
            throw;
        }
    }

    // ── Middleware pipeline ────────────────────────────────────────────────────
    app.UseMiddleware<ExceptionMiddleware>();   // Global exception handler — must be first
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "EcommerceCA API v1");
            c.RoutePrefix        = string.Empty;   // Swagger at root URL
            c.DisplayRequestDuration();
        });
    }

    app.UseStaticFiles();
    app.UseHttpsRedirection();
    app.UseCors("ReactApp");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (HostAbortedException)
{
    throw;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start.");
}
finally
{
    Log.CloseAndFlush();
}
