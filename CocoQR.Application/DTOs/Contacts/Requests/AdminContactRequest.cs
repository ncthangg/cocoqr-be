using CocoQR.Domain.Constants.Enum;

namespace CocoQR.Application.DTOs.Contacts.Requests
{
    public class AdminContactRequest
    {
        public Guid? ContactMessageId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string TemplateKey { get; set; } = string.Empty;
        public string? HtmlBody { get; set; }
        public SmtpSettingType? SmtpType { get; set; }
    }
}