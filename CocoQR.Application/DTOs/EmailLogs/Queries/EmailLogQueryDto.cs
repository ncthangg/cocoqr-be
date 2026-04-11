namespace CocoQR.Application.DTOs.EmailLogs.Queries
{
    public class EmailLogListQueryDto
    {
        public Guid Id { get; set; }
        public Guid? RecipientUserId { get; set; }
        public string? RecipientFullName { get; set; }
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public string SmtpType { get; set; } = string.Empty;
        public string EmailDirection { get; set; } = string.Empty;
        public string? TemplateKey { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class EmailLogQueryDto
    {
        public Guid Id { get; set; }
        public Guid? RecipientUserId { get; set; }
        public string? RecipientFullName { get; set; }
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public string SmtpType { get; set; } = string.Empty;
        public string EmailDirection { get; set; } = string.Empty;
        public string? TemplateKey { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}