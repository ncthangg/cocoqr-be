using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.Contracts.IRateLimit;

namespace CocoQR.API.Middlewares
{
    public class SecurityStampMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityStampMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IUserService userService, IRateLimitService rateLimitService)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userId = context.User.FindFirst("id")?.Value;
            var key = !string.IsNullOrWhiteSpace(userId)
                ? $"rate:user:{userId}"
                : $"rate:ip:{ip}";

            var allowed = await rateLimitService.IsAllowedAsync(
                key,
                50,
                TimeSpan.FromMinutes(1)
            );

            if (!allowed)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                return;
            }

            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdStr = context.User.FindFirst("id")?.Value;
                var tokenStamp = context.User.FindFirst("security_stamp")?.Value;

                if (string.IsNullOrEmpty(userIdStr) || string.IsNullOrEmpty(tokenStamp))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                if (!Guid.TryParse(userIdStr, out var parsedUserId))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                var user = await userService.GetByIdAsync(parsedUserId);

                if (user == null || user.SecurityStamp != tokenStamp || user.Status == false)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
            }

            await _next(context);
        }
    }

}
