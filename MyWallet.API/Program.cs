using Microsoft.EntityFrameworkCore;
using MyWallet.API.DependencyInjection;
using MyWallet.API.Extensions;
using MyWallet.API.Middlewares;
using MyWallet.Application.DependencyInjection;
using MyWallet.Infrastructure.DependencyInjection;
using MyWallet.Infrastructure.Persistence.MyDbContext;

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

if (app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My Portfolios API - STAGING")
    );
}
app.UseForwardedHeaders();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

var forwardOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost
};

// 🔥 CỰC KỲ QUAN TRỌNG
forwardOptions.KnownNetworks.Clear();
forwardOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardOptions);

app.UseCustomForwardedHeaders();

app.UseCustomForwardedHeaders();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<SecurityStampMiddleware>();

app.MapControllers();

app.Run();
