using MyWallet.Application.DTOs.Auths.Responses;
using MyWallet.Domain.Entities;
using System.Security.Claims;

namespace MyWallet.Application.Contracts.ISubServices
{
    public interface ITokenService
    {
        Task<TokenRes> GenerateTokens(Guid userId, IEnumerable<Role> roles, DateTime? expiredTime);
        Task<TokenRes> GenerateNewRefreshTokenAsync(string oldRefreshToken);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        bool IsTokenExpired(string token);
    }
}
