using CocoQR.Domain.Constants.Enum;

namespace CocoQR.Domain.Entities
{
    public class SmtpSetting
    {
        public Guid Id { get; set; }
        public string Host { get; set; } = null!;
        public int Port { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool EnableSSL { get; set; }
        public string FromEmail { get; set; } = null!;
        public string FromName { get; set; } = null!;
        public SmtpSettingType Type { get; set; } = SmtpSettingType.Unknown;
        public bool IsActive { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
