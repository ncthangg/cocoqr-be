namespace CocoQR.Domain.Constants
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
        public const int LongCacheExpiryMinutes = 1440;

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
    public static class DigitalOceanConfig
    {
        public const string Section = "DigitalOcean";
        public const string AccessKeyConfigPath = "DigitalOcean:AccessKey";
        public const string SecretKeyConfigPath = "DigitalOcean:SecretKey";
        public const string BucketConfigPath = "DigitalOcean:Bucket";
        public const string RegionConfigPath = "DigitalOcean:Region";
        public const string EndpointConfigPath = "DigitalOcean:Endpoint";
    }
    public static class CloudinaryConfig
    {
        public const string Section = "Cloudinary";
        public const string ApiKeyConfigPath = "Cloudinary:ApiKey";
        public const string ApiSecretConfigPath = "Cloudinary:ApiSecret";
        public const string CloudNameConfigPath = "Cloudinary:CloudName";
        public const string ProjectNameConfigPath = "Cloudinary:ProjectName";
        public const string BaseUrlConfigPath = "Cloudinary:BaseUrl";

        public const string DefaultBaseUrl = "https://res.cloudinary.com";
        public const string DeliveryTypeUpload = "upload";
        public const string ResourceTypeRaw = "raw";
        public const string ResourceTypeImage = "image";
        public const string ResourceTypeVideo = "video";
    }

    public static class FileUrl
    {
        public const string Section = "FileUrl";
        public const string BaseUrlConfigPath = "FileUrl:BaseUrl";
    }

    public static class FileStorage
    {
        public static string RootPath { get; set; } = string.Empty;

        public const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

        public static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".bmp", ".tiff", ".avif"
        };

        public static readonly HashSet<string> AllowedVideoExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".mp4", ".mov", ".avi", ".wmv", ".mkv", ".webm", ".m4v", ".flv"
        };

        public static readonly HashSet<string> AllowedDocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".csv"
        };
        public static class Folders
        {
            public const string Seed = "Seed";
            public const string Data = "Data";

            public const string Assets = "assets";
            public const string Providers = "providers";
            public const string Banks = "banks";

            public const string Backups = "backups";
            public const string Logs = "logs";

            public const string Info = "information";
            public const string Warning = "warning";
            public const string Error = "error";
        }
        public static class EnvKeys
        {
            public const string Root = "Folder_Root";

            public const string Assets = "Folder_Assets";
            public const string Logs = "Folder_Logs";
            public const string Backups = "Folder_Backups";
        }
    }
}
