namespace CocoQR.Application.Contracts.IConfigs
{
    public interface ITokenConfiguration
    {
        string Issuer { get; }
        string Audience { get; }
        string SecretKey { get; }
        int AccessTokenExpirationMinutes { get; }
        int RefreshTokenExpirationDays { get; }
    }
}
