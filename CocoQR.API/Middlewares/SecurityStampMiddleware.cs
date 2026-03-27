using CocoQR.Application.Contracts.IServices;

namespace CocoQR.API.Middlewares
{
    public class SecurityStampMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityStampMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IUserService userService)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdStr = context.User.FindFirst("id")?.Value;
                var tokenStamp = context.User.FindFirst("security_stamp")?.Value;

                if (string.IsNullOrEmpty(userIdStr) || string.IsNullOrEmpty(tokenStamp))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                if (!Guid.TryParse(userIdStr, out var userId))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                var user = await userService.GetByIdAsync(userId);

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
