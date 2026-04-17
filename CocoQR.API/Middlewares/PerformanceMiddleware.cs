using System.Diagnostics;

namespace CocoQR.API.Middlewares
{
    public class PerformanceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public PerformanceMiddleware(
            RequestDelegate next,
            ILogger<PerformanceMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            var userId = context.User.FindFirst("id")?.Value;

            var ip = context.Connection.RemoteIpAddress?.ToString();

            var userAgent = context.Request.Headers["User-Agent"].ToString();

            var traceId = context.TraceIdentifier;

            await _next(context);

            stopwatch.Stop();

            var elapsed = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {Elapsed} ms | UserId: {UserId} | IP: {IP} | Browser: {Browser} | ENV: {Env} | TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                elapsed,
                userId ?? "Anonymous",
                ip,
                userAgent,
                _env.EnvironmentName,
                traceId
            );
        }
    }
}
