using Microsoft.EntityFrameworkCore;
using CocoQR.API.DependencyInjection;
using CocoQR.API.Extensions;
using CocoQR.API.Middlewares;
using CocoQR.Application.DependencyInjection;
using CocoQR.Infrastructure.DependencyInjection;
using CocoQR.Infrastructure.Persistence.MyDbContext;
using CocoQR.Infrastructure.Persistence.Seeder;
using CocoQR.QR_Generator.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.AddCustomLogging();
builder.Services.AddForwardedHeadersConfig();

// Add services to the container.
builder.Services.AddQrGenerator();
builder.Services.AddPresentation(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (args.Contains("--migrate"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CocoQRDbContext>();
    await db.Database.MigrateAsync();
    return;
}

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            var db = services.GetRequiredService<CocoQRDbContext>();

            logger.LogInformation("Checking for pending database migrations...");

            await db.Database.MigrateAsync();

            logger.LogInformation("Database migrations completed successfully");

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database initialization");

            // throw; 
        }
    }
}

using (var scope = app.Services.CreateScope())
{
    var roleSeeder = scope.ServiceProvider.GetRequiredService<RoleSeeder>();
    await roleSeeder.SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My CocoQR API - DEVELOP")
    );
}

if (app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My CocoQR API - STAGING")
    );
}

app.UseCustomForwardedHeaders();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<SecurityStampMiddleware>();

app.MapControllers();

app.Run();
