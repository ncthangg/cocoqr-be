using Serilog;
using System.Text;

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
            else
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Warning()
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File(
                        "logs/log-.txt",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7,
                        encoding: Encoding.UTF8,
                        outputTemplate:
                        "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
                    )
                    .CreateLogger();

                builder.Host.UseSerilog();
                return builder;
            }
        }
    }
}
