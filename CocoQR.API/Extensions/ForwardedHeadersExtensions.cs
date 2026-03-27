using Microsoft.AspNetCore.HttpOverrides;

namespace CocoQR.API.Extensions
{
    public static class ForwardedHeadersExtensions
    {
        public static IApplicationBuilder UseCustomForwardedHeaders(this IApplicationBuilder app)
        {
            app.UseForwardedHeaders();
            return app;
        }
        public static IServiceCollection AddForwardedHeadersConfig(this IServiceCollection services)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto |
                    ForwardedHeaders.XForwardedHost;

                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            return services;
        }
    }
}
