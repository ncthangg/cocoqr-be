using CocoQR.Application.Contracts.IConfigs;

namespace CocoQR.Infrastructure.Configs
{
    public class DigitalOceanSettings : IDigitalOceanConfiguration
    {
        public string AccessKey { get; set; } = default!;
        public string SecretKey { get; set; } = default!;
        public string Bucket { get; set; } = default!;
        public string Region { get; set; } = default!;
        public string Endpoint { get; set; } = default!;
    }
}
