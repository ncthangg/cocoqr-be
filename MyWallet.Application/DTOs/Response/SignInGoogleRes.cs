namespace MyWallet.Application.DTOs.Response
{
    public class SignInGoogleRes
    {
        public required GetUserRes UserRes { get; set; }
        public required TokenRes TokenRes { get; set; }
        public required RoleRes RoleRes { get; set; }
    }
    public class TokenRes
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
    public class RoleRes
    {
        public string RoleName { get; set; } = string.Empty;
        public string RoleNameUppercase { get; set; } = string.Empty;
    }
}
