using Serilog;
using Serilog.Events;
using System.Text;
using static CocoQR.Domain.Constants.FileStorage;

namespace CocoQR.API.Extensions
{
    public static class LoggingExtensions
    {
        public static WebApplicationBuilder AddCustomLogging(this WebApplicationBuilder builder)
        {
            var env = builder.Environment;

            if (env.IsDevelopment())
            {
                builder.Logging.ClearProviders();
                builder.Logging.AddSimpleConsole(options =>
                {
                    options.TimestampFormat = "HH:mm:ss ";
                    options.SingleLine = true;
                });

                Console.OutputEncoding = Encoding.UTF8;
                return builder;
            }

            var logFolder = ResolveLogFolder();
            Directory.CreateDirectory(Path.Combine(logFolder, Folders.Info));
            Directory.CreateDirectory(Path.Combine(logFolder, Folders.Warning));
            Directory.CreateDirectory(Path.Combine(logFolder, Folders.Error));

            var loggerConfig = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console();

            if (env.IsStaging())
            {
                loggerConfig
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information);
            }
            else if (env.IsProduction())
            {
                loggerConfig
                    .MinimumLevel.Warning()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information);
            }

            if (env.IsStaging() || env.IsProduction())
            {
                loggerConfig
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information)
                    .WriteTo.File(
                        Path.Combine(logFolder, Folders.Info, "info-.txt"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7,
                        encoding: Encoding.UTF8))

                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning)
                    .WriteTo.File(
                        Path.Combine(logFolder, Folders.Warning, "warning-.txt"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7,
                        encoding: Encoding.UTF8))

                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error)
                    .WriteTo.File(
                        Path.Combine(logFolder, Folders.Error, "error-.txt"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7,
                        encoding: Encoding.UTF8));
            }

            Log.Logger = loggerConfig.CreateLogger();
            builder.Host.UseSerilog(Log.Logger, dispose: true);
            return builder;
        }

        private static string ResolveLogFolder()
        {
            var configuredLogPath = Environment.GetEnvironmentVariable(EnvKeys.Logs);
            if (!string.IsNullOrWhiteSpace(configuredLogPath))
            {
                if (Path.IsPathRooted(configuredLogPath))
                {
                    return Path.GetFullPath(configuredLogPath);
                }

                return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configuredLogPath));
            }

            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, Folders.Logs));
        }
    }
}
