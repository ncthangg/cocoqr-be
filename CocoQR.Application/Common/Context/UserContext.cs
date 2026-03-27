using Microsoft.AspNetCore.Http;
using CocoQR.Application.Contracts.IContext;
using CocoQR.Domain.Constants.Enum;
using CocoQR.Domain.Helper;
using System.Security.Claims;

namespace CocoQR.Application.Common.Context
{
    public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        private HttpContext? HttpContext => _httpContextAccessor.HttpContext;

        public Guid? UserId
        {
            get
            {
                var value = HttpContext?.User?.FindFirst("id")?.Value;
                return Guid.TryParse(value, out var id) ? id : null;
            }
        }

        public IEnumerable<string> RoleNames => HttpContext?.User?.FindAll(ClaimTypes.Role).Select(c => c.Value)
                                                 ?? Enumerable.Empty<string>();

        public string? SecurityStamp => HttpContext?.User?.FindFirst("security-stamp")?.Value;

        public string IpAddress
        {
            get
            {
                var ip = HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";

                if (ip == "::1")
                    return "127.0.0.1";

                return ip;
            }
        }

        public string? UserAgent => HttpContext?.Request.Headers["User-Agent"].ToString();

        public string Browser
        {
            get
            {
                var (browser, _) = BrowserHelper.Parse(UserAgent);
                return browser;
            }
        }

        public string BrowserVersion
        {
            get
            {
                var (_, version) = BrowserHelper.Parse(UserAgent);
                return version;
            }
        }

        public string Device
        {
            get
            {
                var ua = UserAgent?.ToLower() ?? "";

                if (ua.Contains("ipad") || ua.Contains("tablet"))
                    return "tablet";

                if (ua.Contains("mobile") || ua.Contains("android") || ua.Contains("iphone"))
                    return "mobile";

                return "desktop";
            }
        }

        public string? Referer => HttpContext?.Request.Headers["Referer"].ToString();

        public string VisitorId
        {
            get
            {
                if (HttpContext == null) return "unknown";

                var ctx = HttpContext;

                if (ctx.Request.Cookies.TryGetValue("visitor_id", out var id) &&
                    !string.IsNullOrWhiteSpace(id))
                {
                    return id;
                }

                // If user doesn't have visitor_id cookie, create one
                id = $"vd_{Guid.NewGuid():N}";

                ctx.Response.Cookies.Append("visitor_id", id, new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddYears(1),
                    HttpOnly = false,
                    SameSite = SameSiteMode.Lax,
                    Secure = false
                });

                return id;
            }
        }

        /// <summary>
        /// Ensures the visitor cookie exists
        /// </summary>
        public void EnsureVisitorCookie()
        {
            if (HttpContext == null) return;

            var ctx = HttpContext;
            var req = ctx.Request;
            var res = ctx.Response;

            if (req.Cookies.ContainsKey("visitor_id"))
                return;

            var id = $"vd_{Guid.NewGuid():N}";

            res.Cookies.Append("visitor_id", id, new CookieOptions
            {
                Expires = DateTime.UtcNow.AddYears(1),
                HttpOnly = false,
                SameSite = SameSiteMode.Lax,
                Secure = false
            });
        }

        public bool IsAuthenticated()
        {
            return UserId.HasValue;
        }

        public bool IsAdmin()
        {
            return IsAuthenticated() && RoleNames.Any(r => Enum.TryParse<RoleCategory>(r, true, out var roleEnum)
                                                          && roleEnum == RoleCategory.ADMIN);
        }
        public bool IsUser()
        {
            return IsAuthenticated() && RoleNames.Any(r => Enum.TryParse<RoleCategory>(r, true, out var roleEnum)
                                                          && roleEnum == RoleCategory.USER);
        }
    }
}
