using CocoQR.Application.Contracts.IConfigs;

namespace CocoQR.Infrastructure.Configs
{
    public sealed class CocoMailOptions : IEmailConfiguration
    {
        public string? BaseUrl { get; set; }
        public string? SystemCode { get; set; }
        public string? KeyId { get; set; }
        public string? PrivateKeyPem { get; set; }
        public string? AdminNotificationEmail { get; set; }
        public int DefaultPriority { get; set; } = 5;
    }
}
