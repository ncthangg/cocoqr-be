using MyWallet.Application.DTOs.Roles.Responses;
using MyWallet.Application.DTOs.Users.Responses;

namespace MyWallet.Application.DTOs.Auths.Responses
{
    public class SignInGoogleRes
    {
        public required GetUserRes UserRes { get; set; }
        public required TokenRes TokenRes { get; set; }
        public required IEnumerable<GetRoleRes> RoleRes { get; set; }
    }
    public class TokenRes
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
