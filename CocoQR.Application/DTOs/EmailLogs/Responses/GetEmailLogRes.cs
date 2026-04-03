using CocoQR.Domain.Constants.Enum;

namespace CocoQR.Application.DTOs.EmailLogs.Responses
{
    public class GetEmailLogRes
    {
        public Guid Id { get; set; }
        public Guid? RecipientUserId { get; set; }
        public string? RecipientFullName { get; set; }
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public EmailLogStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public SmtpSettingType SmtpType { get; set; }
        public EmailDirection EmailDirection { get; set; }
        public string? TemplateKey { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class GetEmailLogByIdRes
    {
        public Guid Id { get; set; }
        public Guid? RecipientUserId { get; set; }
        public string? RecipientFullName { get; set; }
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public EmailLogStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public SmtpSettingType SmtpType { get; set; }
        public EmailDirection EmailDirection { get; set; }
        public string? TemplateKey { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}