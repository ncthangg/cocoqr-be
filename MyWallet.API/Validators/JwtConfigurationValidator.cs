using MyWallet.API.Configurations;
using MyWallet.Domain.Constants;
using ApplicationException = MyWallet.Application.Exceptions.ApplicationException;


namespace MyWallet.API.Validators
{
    public static class JwtConfigurationValidator
    {
        private const int MinimumSecretKeyLength = 32;
        private const int MaxRecommendedAccessTokenMinutes = 1440; // 24 hours
        private const int MinRefreshTokenDays = 1;

        /// <summary>
        /// Validates the JWT configuration and throws exception if invalid
        /// </summary>
        /// <exception cref="ApplicationException">Thrown when configuration is invalid</exception>
        public static void Validate(JwtConfiguration config)
        {
            ValidateSecretKey(config.SecretKey);
            ValidateIssuer(config.Issuer);
            ValidateAudience(config.Audience);
            ValidateAccessTokenExpiration(config.AccessTokenExpirationMinutes);
            ValidateRefreshTokenExpiration(config.RefreshTokenExpirationDays);
        }

        /// <summary>
        /// Validates and returns the configuration (fluent style)
        /// </summary>
        public static JwtConfiguration ValidateAndReturn(JwtConfiguration config)
        {
            Validate(config);
            return config;
        }

        private static void ValidateSecretKey(string secretKey)
        {
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new ApplicationException(
                    ErrorCode.BadRequest,
                    "JWT SecretKey is required and cannot be empty. Please configure 'Jwt:SecretKey' in appsettings.json"
                );
            }

            if (secretKey.Length < MinimumSecretKeyLength)
            {
                throw new ApplicationException(
                    ErrorCode.BadRequest,
                    $"JWT SecretKey must be at least {MinimumSecretKeyLength} characters long for security. Current length: {secretKey.Length}"
                );
            }
        }

        private static void ValidateIssuer(string issuer)
        {
            if (string.IsNullOrWhiteSpace(issuer))
            {
                throw new ApplicationException(
                    ErrorCode.BadRequest,
                    "JWT Issuer is required and cannot be empty. Please configure 'Jwt:Issuer' in appsettings.json"
                );
            }
        }

        private static void ValidateAudience(string audience)
        {
            if (string.IsNullOrWhiteSpace(audience))
            {
                throw new ApplicationException(
                    ErrorCode.BadRequest,
                    "JWT Audience is required and cannot be empty. Please configure 'Jwt:Audience' in appsettings.json"
                );
            }
        }

        private static void ValidateAccessTokenExpiration(int minutes)
        {
            if (minutes <= 0)
            {
                throw new ApplicationException(
                    ErrorCode.BadRequest,
                    $"JWT AccessTokenExpirationMinutes must be greater than 0. Current value: {minutes}"
                );
            }

            // Note: We could log a warning if > MaxRecommendedAccessTokenMinutes
            // but we'll allow it for flexibility
        }

        private static void ValidateRefreshTokenExpiration(int days)
        {
            if (days < MinRefreshTokenDays)
            {
                throw new ApplicationException(
                    ErrorCode.BadRequest,
                    $"JWT RefreshTokenExpirationDays must be at least {MinRefreshTokenDays} day(s). Current value: {days}"
                );
            }
        }
    }
}
