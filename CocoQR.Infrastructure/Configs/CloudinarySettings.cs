using CocoQR.Application.Contracts.IConfigs;

namespace CocoQR.Infrastructure.Configs
{
    public class CloudinarySettings : ICloudinaryConfiguration
    {
        public string CloudName { get; set; } = default!;
        public string ApiKey { get; set; } = default!;
        public string ApiSecret { get; set; } = default!;
        public string ProjectName { get; set; } = default!;
        public string BaseUrl { get; set; } = default!;
    }
}