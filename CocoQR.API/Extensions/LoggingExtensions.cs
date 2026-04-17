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

            var loggerConfig = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .Enrich.WithProcessName()
                .Enrich.WithThreadId()
                .Enrich.WithEnvironmentName();

            var outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] " +
                                 "[{SourceContext}] " + 
                                 "[MachineName:{MachineName}] " +
                                 "[ThreadId:{ThreadId}] " +
                                 "[EnvironmentName:{EnvironmentName}] " +
                                 "{Message:lj} " +
                                 "{NewLine}{Exception}";

            Console.OutputEncoding = Encoding.UTF8;

            if (env.IsDevelopment())
            {
                loggerConfig.WriteTo.Console(outputTemplate: outputTemplate);
            }
            else
            {
                var logFolder = ResolveLogFolder();

                Directory.CreateDirectory(Path.Combine(logFolder, Folders.Info));
                Directory.CreateDirectory(Path.Combine(logFolder, Folders.Warning));
                Directory.CreateDirectory(Path.Combine(logFolder, Folders.Error));

                loggerConfig.WriteTo.Console(outputTemplate: outputTemplate);

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
