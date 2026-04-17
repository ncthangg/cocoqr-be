using CocoQR.Application.Contracts.IConfigs;
using CocoQR.Application.Contracts.IContext;
using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Application.DTOs.Auths.Responses;
using CocoQR.Domain.Constants;
using CocoQR.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApplicationException = CocoQR.Application.Exceptions.ApplicationException;

namespace CocoQR.Infrastructure.Security
{
    public class TokenService : ITokenService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ITokenConfiguration _tokenConfig;
        private readonly IUserContext _userContext;

        public TokenService(IUnitOfWork unitOfWork,
            ITokenConfiguration tokenConfig,
            IUserContext userContext)
        {
            _unitOfWork = unitOfWork;

            _tokenConfig = tokenConfig;
            _userContext = userContext;
        }

        public async Task<TokenRes> GenerateTokens(Guid userId, IEnumerable<Role> roles, DateTime? expiredTime, int? accessTokenLifetimeMinutes = null)
        {
            var now = DateTime.UtcNow;

            var user = await _unitOfWork.Users.GetByIdAsync(userId)
                ?? throw new ApplicationException(ErrorCode.NotFound, ErrorMessages.UserNotFound);

            var claims = new List<Claim> {
                new("id", userId.ToString()),
                new("security_stamp", user.SecurityStamp.ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name.Trim().ToLower()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenConfig.SecretKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var accessToken = new JwtSecurityToken(
                issuer: _tokenConfig.Issuer,
                audience: _tokenConfig.Audience,
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(accessTokenLifetimeMinutes ?? _tokenConfig.AccessTokenExpirationMinutes),
                signingCredentials: creds
            );

            var refreshToken = new JwtSecurityToken(
                issuer: _tokenConfig.Issuer,
                audience: _tokenConfig.Audience,
                claims: claims,
                notBefore: now,
                expires: expiredTime ?? now.AddDays(_tokenConfig.RefreshTokenExpirationDays),
                signingCredentials: creds
            );

            return new TokenRes
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
                RefreshToken = new JwtSecurityTokenHandler().WriteToken(refreshToken)
            };
        }
        public async Task<TokenRes> GenerateNewRefreshTokenAsync(string oldRefreshToken)
        {
            var userId = _userContext.UserId
                ?? throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.UserIDNotFoundInTheContext);

            var roles = await _unitOfWork.UserRoles.GetRolesByUserIdAsync(userId)
                ?? throw new ApplicationException(ErrorCode.NotFound, $"Role of {userId} not found");

            DateTime expiredTime;
            try
            {
                expiredTime = DecodeOldRefreshToken(oldRefreshToken);
            }
            catch
            {
                throw new ApplicationException(ErrorCode.ValidationError, "Invalid old refresh token.");
            }

            var userToken = await _unitOfWork.UserTokens.GetByIdAsync(userId);
            TokenRes tokensVM = await GenerateTokens(userId, roles, expiredTime, null);
            if (userToken != null)
            {
                userToken.RefreshToken = tokensVM.RefreshToken;
                userToken.ExpiredTime = expiredTime;
                await _unitOfWork.CommitAsync();
            }
            return new TokenRes
            {
                AccessToken = tokensVM.AccessToken,
                RefreshToken = tokensVM.RefreshToken
            };
        }
        private static DateTime DecodeOldRefreshToken(string oldRefreshToken)
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(oldRefreshToken))
                throw new ApplicationException(ErrorCode.ValidationError, "Invalid token format.");

            var jwtToken = handler.ReadJwtToken(oldRefreshToken);
            var expiredClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)
                ?? throw new ApplicationException(ErrorCode.ValidationError, "Token does not contain expiry information.");
            var expiredTimeUnix = long.Parse(expiredClaim.Value);
            var expiredTime = DateTimeOffset.FromUnixTimeSeconds(expiredTimeUnix).UtcDateTime;

            return expiredTime;
        }
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenConfig.SecretKey!)),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false, // We allow expired tokens to validate the user
                ValidIssuer = _tokenConfig.Issuer,
                ValidAudience = _tokenConfig.Audience,
            };
            try
            {
                var principal = tokenHandler.ValidateToken(token, parameters, out _);
                return principal;
            }
            catch
            {
                return null!;
            }
        }

        public bool IsTokenExpired(string token)
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return jwtToken.ValidTo < DateTime.UtcNow;
        }
    }
}
