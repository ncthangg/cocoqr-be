namespace CocoQR.Application.Contracts.IConfigs
{
    public interface IDigitalOceanConfiguration
    {
        string AccessKey { get; }
        string SecretKey { get; }
        string Bucket { get; }
        string Region { get; }
        string Endpoint { get; }
    }
}
