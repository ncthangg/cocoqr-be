using CocoQR.Domain.Constants.Enum;

namespace CocoQR.Application.DTOs.Settings
{
    public class GetSmtpSettingRes
    {
        public Guid Id { get; set; }
        public SmtpSettingType Type { get; set; }
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool HasPassword { get; set; }
        public bool EnableSSL { get; set; }
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
