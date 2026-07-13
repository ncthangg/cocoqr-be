using CocoQR.Application.Contracts.IConfigs;

namespace CocoQR.Infrastructure.Configs
{
    public sealed class CocoMailOptions : IEmailConfiguration
    {
        public string? BaseUrl { get; set; }
        public string? SendEndpoint { get; set; }
        public string? TemplateEndpoint { get; set; }
        public string? SystemCode { get; set; }
        public string? KeyId { get; set; }
        public string? PrivateKeyBase64 { get; set; }
        public string? AdminNotificationEmail { get; set; }
        public string? CallbackUrl { get; set; }
        public int DefaultPriority { get; set; } = 5;
        public int TimeoutSeconds { get; set; } = 30;
    }
}
