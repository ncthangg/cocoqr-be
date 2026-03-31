using CocoQR.Application.Common.Context;
using CocoQR.Application.Contracts.IConfigs;
using CocoQR.Application.Contracts.IContext;
using CocoQR.Application.Contracts.IRepositories;
using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Domain.Constants;
using CocoQR.Infrastructure.BackgroundServices;
using CocoQR.Infrastructure.Persistence.MyDbContext;
using CocoQR.Infrastructure.Persistence.Repositories;
using CocoQR.Infrastructure.Persistence.Seeder;
using CocoQR.Infrastructure.Persistence.UnitOfWork;
using CocoQR.Infrastructure.Security;
using CocoQR.Infrastructure.SubService;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using IDbConnectionFactory = CocoQR.Application.Contracts.IDbContext.IDbConnectionFactory;

namespace CocoQR.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
        {
            services.AddDbConnectionFactory();
            services.AddRepo();
            services.AddDatabase(configuration);
            services.AddSubServices();
            services.AddBackgroundServices(env);
            services.AddSeeder();
            services.AddJWTConfig(configuration);
            services.AddCloudConfig(configuration);
            services.ConfigRedis(configuration);
        }
        private static void AddDbConnectionFactory(this IServiceCollection services)
        {
            services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
        }
        private static void AddRepo(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register specific repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IAccountRepository, AccountRepository>();

            services.AddScoped<IQrRepository, QrRepository>();
            services.AddScoped<IQRStyleLibraryRepository, QRStyleLibraryRepository>();

            services.AddScoped<IBankInfoRepository, BankInfoRepository>();
            services.AddScoped<IProviderRepository, ProviderRepository>();

            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<IUserTokenRepository, UserTokenRepository>();
        }
        private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<CocoQRDbContext>((sp, options) =>
            {
                options.UseSqlServer(configuration.GetConnectionString(Database.DefaultConnection),
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null);
                    });
            });
        }
        private static void AddSubServices(this IServiceCollection services)
        {
            services.AddScoped<IUserContext, UserContext>();

            services.AddScoped<IGoogleService, GoogleService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddSingleton<IFileCleanupQueue, FileCleanupQueue>();

            // Default cloud provider: DigitalOcean Spaces.
            // Switch to Cloudinary by replacing this registration with:
            services.AddScoped<ICloudStorage, CloudinaryStorage>();
            // services.AddScoped<ICloudStorage, DigitalOceanStorage>();

            services.AddScoped<FileStorageService>();
            services.AddScoped<IFileStorageService>(sp => sp.GetRequiredService<FileStorageService>());
            services.AddScoped<IIdGenerator, SqlServerIdGenerator>();
        }
        private static void AddBackgroundServices(this IServiceCollection services, IHostEnvironment env)
        {
            if (env.IsStaging() || env.IsProduction())
            {
                services.AddHostedService<FileCleanupBackgroundService>();
                services.AddHostedService<LogUploadService>();
            }
        }
        private static void AddSeeder(this IServiceCollection services)
        {
            services.AddScoped<RoleSeeder>();
            services.AddScoped<BankSeeder>();
            services.AddScoped<ProviderSeeder>();
            // optionally register QR style seeder
        }
        private static void AddJWTConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<TokenConfiguration>(configuration.GetSection(Jwt.JwtConst));
            services.AddScoped<ITokenConfiguration>(sp => sp.GetRequiredService<IOptions<TokenConfiguration>>().Value);
        }
        private static void AddCloudConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DigitalOceanSettings>(configuration.GetSection("DigitalOcean"));
            services.Configure<CloudinarySettings>(configuration.GetSection("Cloudinary"));
        }
        private static IServiceCollection ConfigRedis(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var connectionString = configuration[Redis.RedisConnection];

                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new InvalidOperationException(
                        $"Redis connection string '{Redis.RedisConnection}' is not configured.");

                // --- Cấu hình bổ sung ---
                var options = ConfigurationOptions.Parse(connectionString);
                options.AbortOnConnectFail = configuration.GetValue<bool>(Redis.AbortOnConnectFail);

                options.ConnectRetry = configuration.GetValue<int>(Redis.ConnectRetry);

                options.ConnectTimeout = configuration.GetValue<int>(Redis.ConnectTimeoutMs);

                options.SyncTimeout = configuration.GetValue<int>(Redis.SyncTimeoutMs);

                options.ReconnectRetryPolicy = new LinearRetry(configuration.GetValue<int>(Redis.ReconnectRetryIntervalMs));

                return ConnectionMultiplexer.Connect(options);
            });

            return services;
        }

    }
}
