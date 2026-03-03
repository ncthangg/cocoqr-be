using MyWallet.Application.Contracts.IServices;

namespace MyWallet.API.Middlewares
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
            //if (context.User.Identity?.IsAuthenticated == true)
            //{
            //    var userId = context.User.FindFirst("id")?.Value;
            //    var tokenStamp = context.User.FindFirst("security_stamp")?.Value;

            //    if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tokenStamp))
            //    {
            //        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            //        return;
            //    }

            //    //var user = await userService.ToString(userId);

            //    //if (user == null || user.SecurityStamp != tokenStamp)
            //    //{
            //    //    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            //    //    return;
            //    //}
            //}

            await _next(context);
        }
    }

}
