using CocoQR.Application.Common.Extensions;
using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Application.DTOs.Auths.Requests;
using CocoQR.Application.DTOs.Auths.Responses;
using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Application.DTOs.Users.Responses;
using CocoQR.Domain.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CocoQR.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthsController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IGoogleService _googleService;
        private readonly ILogger<AuthsController> _logger;
        public AuthsController(
            IAuthService authService,
            IGoogleService googleService,
            ILogger<AuthsController> logger)
        {
            _authService = authService;
            _googleService = googleService;
            _logger = logger;
        }

        [HttpGet("google-auth/signin")]
        [AllowAnonymous]
        public IActionResult SignIn([FromQuery] string origin)
        {
            if (string.IsNullOrWhiteSpace(origin))
                return BadRequest("Origin is required");

            try
            {
                _logger.LogInformation(
                    "Google sign-in requested. Origin: {Origin}, Host: {Host}, Scheme: {Scheme}",
                    origin,
                    Request.Host,
                    Request.Scheme);

                // ✅ Clear cookies
                Response.Cookies.Delete("GoogleOAuthTemp");
                Response.Cookies.Delete(".AspNetCore.Cookies");

                // ✅ Use extension method (single source of truth)
                var redirectUri = Request.GetCallbackUrl(origin);

                _logger.LogDebug("Google sign-in redirect URI generated: {RedirectUri}", redirectUri);

                var props = new AuthenticationProperties
                {
                    RedirectUri = redirectUri,
                    IsPersistent = false
                };

                return Challenge(props, GoogleDefaults.AuthenticationScheme);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Google sign-in endpoint failed for origin: {Origin}", origin);
                return BadRequest(new
                {
                    error = ex.Message
                });
            }
        }

        [HttpGet("google-auth/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleCallback([FromQuery] string origin)
        {
            if (string.IsNullOrWhiteSpace(origin))
                return BadRequest("Origin is required");

            try
            {
                // ✅ Check if authentication succeeded
                var result = await HttpContext.AuthenticateAsync("OAuthTemp");

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Google callback auth failed: {Failure}", result.Failure?.Message);
                    return Redirect($"{origin}?error=auth_failed");
                }

                _logger.LogInformation("Google callback authentication succeeded. Processing user profile.");

                // ✅ Now process the authenticated user
                var signInResult = await _authService.SignInGoogle(HttpContext);

                var response = new BaseResponseModel<SignInGoogleRes>(
                    SuccessCode.Success,
                    signInResult,
                    "Đăng nhập Google thành công!"
                );

                var html = _googleService.BuildSuccessHtml(response, origin);
                _logger.LogInformation("Google callback completed successfully for origin: {Origin}", origin);

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google callback processing failed for origin: {Origin}", origin);
                return Redirect($"{origin}?error={Uri.EscapeDataString(ex.Message)}");
            }
        }

        [HttpPost("switch-role")]
        [Authorize]
        public async Task<IActionResult> SwitchRole([FromBody] SwitchRoleReq request)
        {
            var result = await _authService.SwitchRoleAsync(request);
            return Ok(new BaseResponseModel<SwitchRoleRes>(
                code: SuccessCode.Success,
                message: null,
                data: result));
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            GetUserRes result = await _authService.Me();
            return Ok(new BaseResponseModel<GetUserRes>(
                               code: SuccessCode.Success,
                               message: null,
                               data: result
                               ));
        }

        [HttpPost("signout")]
        [Authorize]
        public async Task<IActionResult> SignOutUser()
        {
            try
            {
                // ✅ Logout from OAuthTemp cookie
                await HttpContext.SignOutAsync("OAuthTemp");

                // ✅ Clear all cookies related to Google
                Response.Cookies.Delete("GoogleOAuthTemp");
                Response.Cookies.Delete(".AspNetCore.Cookies");

                return Ok(new BaseResponseModel<string>(code: SuccessCode.Success,
                                                        data: null,
                                                        message: "Đăng xuất thành công!"
                                                        ));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Google signout failed for user: {UserId}", User.FindFirst("id")?.Value);
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
