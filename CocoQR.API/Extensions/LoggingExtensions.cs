using System.Text;

namespace CocoQR.API.Extensions
{
    public static class LoggingExtensions
    {
        public static WebApplicationBuilder AddCustomLogging(this WebApplicationBuilder builder)
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
    }
}
