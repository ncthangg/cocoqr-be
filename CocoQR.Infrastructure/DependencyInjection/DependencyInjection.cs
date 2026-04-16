using CocoQR.Application.Common.Context;
using CocoQR.Application.Contracts.ICache;
using CocoQR.Application.Contracts.IConfigs;
using CocoQR.Application.Contracts.IContext;
using CocoQR.Application.Contracts.IQueue;
using CocoQR.Application.Contracts.IRateLimit;
using CocoQR.Application.Contracts.IRepositories;
using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Domain.Constants;
using CocoQR.Infrastructure.BackgroundServices.BackgroundQueueWorker;
using CocoQR.Infrastructure.BackgroundServices.BackgroundQueueWorker.Handlers;
using CocoQR.Infrastructure.BackgroundServices.FileCleanupJob;
using CocoQR.Infrastructure.BackgroundServices.LogUploadJob;
using CocoQR.Infrastructure.Configs;
using CocoQR.Infrastructure.Persistence.MyDbContext;
using CocoQR.Infrastructure.Persistence.Repositories;
using CocoQR.Infrastructure.Persistence.Seeder;
using CocoQR.Infrastructure.Persistence.UnitOfWork;
using CocoQR.Infrastructure.Redis.Cache;
using CocoQR.Infrastructure.Redis.Queue;
using CocoQR.Infrastructure.Redis.RateLimit;
using CocoQR.Infrastructure.Security;
using CocoQR.Infrastructure.SubService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using IDbConnectionFactory = CocoQR.Application.Contracts.IDbContext.IDbConnectionFactory;
using RedisConfig = CocoQR.Domain.Constants.Redis;

namespace CocoQR.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
        {
            services.AddDbConnectionFactory();
            services.AddRepo();
            services.AddDatabaseConfig(configuration);
            services.AddSubServices();
            services.AddBackgroundServices(env);
            services.AddSeeder();
            services.AddJWTConfig(configuration);
            services.AddCloudConfig(configuration);
            services.ConfigRedis(configuration);
            services.AddRedisServices();

            return services;
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
            services.AddScoped<IContactMessageRepository, ContactMessageRepository>();
            services.AddScoped<IEmailLogRepository, EmailLogRepository>();
            services.AddScoped<ISmtpSettingRepository, SmtpSettingRepository>();
            services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();

            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<IUserTokenRepository, UserTokenRepository>();
        }
        private static void AddDatabaseConfig(this IServiceCollection services, IConfiguration configuration)
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
            services.AddScoped<IEmailService, EmailService>();
            services.AddSingleton<IFileCleanupQueue, FileCleanupQueue>();
            services.AddScoped<IBackgroundJobProducer, RedisBackgroundJobProducer>();

            services.AddScoped<UploadLogHandler>();
            services.AddScoped<CleanupHandler>();
            services.AddScoped<UploadAssetHandler>();
            services.AddScoped<EmailHandler>();

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
            services.AddHostedService<BackgroundQueueWorker>();
            if (env.IsStaging() || env.IsProduction())
            {
                services.AddHostedService<FileCleanupJobProducerService>();
                services.AddHostedService<LogUploadJobProducerService>();
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

            services.AddScoped<IDigitalOceanConfiguration>(sp => sp.GetRequiredService<IOptions<DigitalOceanSettings>>().Value);
            services.AddScoped<ICloudinaryConfiguration>(sp => sp.GetRequiredService<IOptions<CloudinarySettings>>().Value);
        }
        private static void ConfigRedis(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("Redis");

                var connectionString = configuration[RedisConfig.RedisConnection];

                try
                {
                    if (string.IsNullOrWhiteSpace(connectionString))
                        throw new ArgumentException(string.Format(ErrorMessages.ConfigurationValueRequired, RedisConfig.RedisConnection),
                                                     RedisConfig.RedisConnection);

                    var options = ConfigurationOptions.Parse(connectionString);

                    options.Password = configuration.GetValue<string>(RedisConfig.Password);
                    options.AbortOnConnectFail = configuration.GetValue<bool>(RedisConfig.AbortOnConnectFail);
                    options.ConnectRetry = configuration.GetValue<int>(RedisConfig.ConnectRetry);
                    options.ConnectTimeout = configuration.GetValue<int>(RedisConfig.ConnectTimeoutMs);
                    options.SyncTimeout = configuration.GetValue<int>(RedisConfig.SyncTimeoutMs);
                    options.AsyncTimeout = configuration.GetValue<int>(RedisConfig.ASyncTimeoutMs);

                    options.ReconnectRetryPolicy = new LinearRetry(configuration.GetValue<int>(RedisConfig.ReconnectRetryIntervalMs));

                    logger.LogInformation("Connecting to Redis...");

                    var redis = ConnectionMultiplexer.Connect(options);

                    if (redis.IsConnected)
                    {
                        logger.LogInformation("Connected to Redis successfully");
                    }
                    else
                    {
                        logger.LogWarning("Redis multiplexer created but not connected yet");
                    }

                    return redis;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Redis connection failed in {Environment}. Running without Redis",
                                          Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));

                    var fallbackOptions = ConfigurationOptions.Parse(connectionString ?? "localhost:6379");
                    fallbackOptions.AbortOnConnectFail = false;
                    fallbackOptions.ConnectRetry = 5;

                    return ConnectionMultiplexer.Connect(fallbackOptions);
                }
            });
        }
        private static void AddRedisServices(this IServiceCollection services)
        {
            services.AddSingleton<ICacheService, RedisCacheService>();
            services.AddSingleton<IQueueService, RedisQueueService>();
            services.AddSingleton<IRateLimitService, RedisRateLimitService>();
        }

    }
}
