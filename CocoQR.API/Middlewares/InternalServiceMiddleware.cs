namespace CocoQR.API.Middlewares
{
    public class InternalServiceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _apiKey;

        public InternalServiceMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _apiKey = configuration["InternalApi:ApiKey"]
                ?? throw new InvalidOperationException("Internal API key is not configured");
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/api/internal"))
            {
                var key = context.Request.Headers["x-api-key"];

                if (string.IsNullOrEmpty(_apiKey) || key != _apiKey)
                {
                    context.Response.StatusCode = 401;
                    return;
                }
            }

            await _next(context);
        }
    }
}
