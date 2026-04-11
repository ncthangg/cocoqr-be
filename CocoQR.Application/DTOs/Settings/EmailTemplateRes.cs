namespace CocoQR.Application.DTOs.Settings
{
    public class GetEmailTemplateRes
    {
        public Guid Id { get; set; }
        public string TemplateKey { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
