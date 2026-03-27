using CocoQR.Application.DTOs.Roles.Responses;
using CocoQR.Application.DTOs.Users.Responses;

namespace CocoQR.Application.DTOs.Auths.Responses
{
    public class SignInGoogleRes
    {
        public required Guid UserId { get; set; }
        public required GetUserRes UserRes { get; set; }
        public TokenRes? TokenRes { get; set; }
        public required IEnumerable<GetRoleRes> RoleRes { get; set; }
    }

    public class SwitchRoleRes
    {
        public required TokenRes TokenRes { get; set; }
        public required GetRoleRes RoleRes { get; set; }
    }

    public class TokenRes
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
