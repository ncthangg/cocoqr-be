namespace MyWallet.Domain.Constants
{
    public static class ConfigOrigins
    {
        public const string AllowedOrigins = "Auth:AllowedOrigins";
    }
    public static class Jwt
    {
        public const string JwtConst = "JWT";
        public const string SecretKeyConfigPath = "JWT:SecretKey";
        public const string IssuerConfigPath = "JWT:Issuer";
        public const string AudienceConfigPath = "JWT:Audience";
        public const string AccessTokenExpirationConfigPath = "JWT:AccessTokenExpirationMinutes";
        public const string RefreshTokenExpirationConfigPath = "JWT:RefreshTokenExpirationDays";
    }
    public static class Database
    {
        public const string DefaultConnection = "DefaultConnection";
        public const string CaseInsensitiveCollation = "Latin1_General_CI_AI";
        public const int DefaultCommandTimeoutSeconds = 30;
        public const int LongRunningCommandTimeoutSeconds = 120;
    }
    public static class Redis
    {
        public const string RedisConnection = "Redis:ConnectionString";

        public const string OtpKeyPrefix = "otp";
        public const string ResetTokenPrefix = "reset";
        public const string SessionPrefix = "session";
        public const string CachePrefix = "cache";

        public const int DefaultCacheExpiryMinutes = 60;
        public const int ShortCacheExpiryMinutes = 5;
        public const int LongCacheExpiryMinutes = 1440; // 24 hours

        // --- Thêm các hằng số cấu hình kết nối ---
        public const string AbortOnConnectFail = "Redis:AbortOnConnectFail";
        public const string ConnectRetry = "Redis:ConnectRetry";
        public const string ConnectTimeoutMs = "Redis:ConnectTimeoutMs";
        public const string SyncTimeoutMs = "Redis:SyncTimeoutMs";
        public const string ReconnectRetryIntervalMs = "Redis:ReconnectRetryIntervalMs";
    }
    public static class Google
    {
        public const string OAuthTempConfigPath = "OAuthTemp";
        public const string ClientIdConfigPath = "Oauth:ClientId";
        public const string ClientSecretConfigPath = "Oauth:ClientSecret";
    }
}
