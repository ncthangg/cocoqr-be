using Microsoft.AspNetCore.Http;
using CocoQR.Application.DTOs.Auths.Requests;
using CocoQR.Application.DTOs.Auths.Responses;
using CocoQR.Application.DTOs.Users.Responses;

namespace CocoQR.Application.Contracts.IServices
{
    public interface IAuthService
    {
        Task<SignInGoogleRes> SignInGoogle(HttpContext httpContext);
        Task<GetUserRes> Me();
        Task<SwitchRoleRes> SwitchRoleAsync(SwitchRoleReq request);
    }
}
