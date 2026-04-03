namespace CocoQR.Application.Contracts.IConfigs
{
    public interface ICloudinaryConfiguration
    {
        string CloudName { get; }
        string ApiKey { get; }
        string ApiSecret { get; }
        string ProjectName { get; }
        string BaseUrl { get; }
    }
}
