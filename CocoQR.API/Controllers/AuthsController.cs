using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CocoQR.Application.Common.Extensions;
using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Application.DTOs.Auths.Requests;
using CocoQR.Application.DTOs.Auths.Responses;
using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Application.DTOs.Users.Responses;
using CocoQR.Domain.Constants;

namespace CocoQR.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthsController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IGoogleService _googleService;
        public AuthsController(IAuthService authService, IGoogleService googleService)
        {
            _authService = authService;
            _googleService = googleService;
        }

        [HttpGet("google-auth/signin")]
        [AllowAnonymous]
        public IActionResult SignIn([FromQuery] string origin)
        {
            if (string.IsNullOrWhiteSpace(origin))
                return BadRequest("Origin is required");

            try
            {
                Console.WriteLine($"\n=== SignIn Called ===");
                Console.WriteLine($"Origin: {origin}");
                Console.WriteLine($"Request Host: {Request.Host}");
                Console.WriteLine($"Request Scheme: {Request.Scheme}");
                Console.WriteLine($"X-Forwarded-Proto: {Request.Headers["X-Forwarded-Proto"]}");
                Console.WriteLine($"X-Forwarded-Host: {Request.Headers["X-Forwarded-Host"]}");

                // ✅ Clear cookies
                Response.Cookies.Delete("GoogleOAuthTemp");
                Response.Cookies.Delete(".AspNetCore.Cookies");

                // ✅ Use extension method (single source of truth)
                var redirectUri = Request.GetCallbackUrl(origin);

                Console.WriteLine($"BaseUrl: {Request.GetBaseUrl()}");
                Console.WriteLine($"RedirectUri: {redirectUri}");
                Console.WriteLine("====================\n");

                var props = new AuthenticationProperties
                {
                    RedirectUri = redirectUri,
                    IsPersistent = false
                };

                return Challenge(props, GoogleDefaults.AuthenticationScheme);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
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
                    Console.WriteLine($"Authentication failed: {result.Failure?.Message}");
                    return Redirect($"{origin}?error=auth_failed");
                }

                Console.WriteLine($"Authentication succeeded. Processing user...");

                // ✅ Now process the authenticated user
                var signInResult = await _authService.SignInGoogle(HttpContext);

                var response = new BaseResponseModel<SignInGoogleRes>(
                    SuccessCode.Success,
                    signInResult,
                    "Đăng nhập Google thành công!"
                );

                var html = _googleService.BuildSuccessHtml(response, origin);

                Console.WriteLine($"Returning success HTML");

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignInGoogle error: {ex.Message}\n{ex.StackTrace}");
                return Redirect($"{origin}?error={Uri.EscapeDataString(ex.Message)}");
            }
        }

        [HttpPost("switch-role")]
        [AllowAnonymous]
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
        public async Task<IActionResult> SignOut()
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
                Console.WriteLine($"Logout error: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
