using Microsoft.AspNetCore.Http;
using MyWallet.Application.DTOs.Auths.Requests;
using MyWallet.Application.DTOs.Auths.Responses;
using MyWallet.Application.DTOs.Users.Responses;

namespace MyWallet.Application.Contracts.IServices
{
    public interface IAuthService
    {
        Task<SignInGoogleRes> SignInGoogle(HttpContext httpContext);
        Task<GetUserRes> Me();
        Task<SwitchRoleRes> SwitchRoleAsync(SwitchRoleReq request);
    }
}
