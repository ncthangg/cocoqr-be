using Microsoft.EntityFrameworkCore;
using MyWallet.API.DependencyInjection;
using MyWallet.API.Extensions;
using MyWallet.API.Middlewares;
using MyWallet.Application.DependencyInjection;
using MyWallet.Infrastructure.DependencyInjection;
using MyWallet.Infrastructure.Persistence.MyDbContext;
using MyWallet.Infrastructure.Persistence.Seeder;

var builder = WebApplication.CreateBuilder(args);

builder.AddCustomLogging();
builder.Services.AddForwardedHeadersConfig();

// Add services to the container.
builder.Services.AddPresentation(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (args.Contains("--migrate"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MyWalletDbContext>();
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
            var db = services.GetRequiredService<MyWalletDbContext>();

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

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My Portfolios API - DEVELOP")
    );
}

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<BankSeeder>();
    await seeder.SeedAsync();
}

if (app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My Portfolios API - STAGING")
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
