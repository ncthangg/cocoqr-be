using CocoQR.Domain.Constants.Enum;

namespace CocoQR.Domain.Entities
{
    public class EmailLog
    {
        public Guid Id { get; set; }
        public string ToEmail { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;
        public EmailLogStatus Status { get; set; }
        public SmtpSettingType SmtpType { get; set; }
        public EmailDirection EmailDirection { get; set; } = EmailDirection.OUTGOING;
        public string? TemplateKey { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
