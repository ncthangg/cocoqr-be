namespace MyWallet.API.Configurations
{
    public class JwtConfiguration
    {
        public const string SectionName = "Jwt";

        public string SecretKey { get; set; } = string.Empty;

        public string Issuer { get; set; } = string.Empty;

        public string Audience { get; set; } = string.Empty;

        public int AccessTokenExpirationMinutes { get; set; } = 60;

        public int RefreshTokenExpirationDays { get; set; } = 7;

        public TimeSpan GetAccessTokenExpiration() =>
            TimeSpan.FromMinutes(AccessTokenExpirationMinutes);

        public TimeSpan GetRefreshTokenExpiration() =>
            TimeSpan.FromDays(RefreshTokenExpirationDays);
    }
}
