namespace CocoQR.Application.Contracts.ISubServices
{
    public interface IEmailService
    {
        Task<MailGatewaySendResponse?> SendAsync(
            string to,
            string subject,
            string body,
            CancellationToken cancellationToken = default);
    }

    public interface ICocoMailClient
    {
        Task<MailGatewaySendResponse?> SendAsync(
            CocoMailEmailRequest email,
            CancellationToken cancellationToken = default);
    }

    public sealed class CocoMailGatewayRequest
    {
        public required CocoMailAuthentication Authentication { get; set; }
        public required CocoMailEmailRequest Email { get; set; }
    }

    public sealed class CocoMailAuthentication
    {
        public required string SystemCode { get; set; }
        public required string KeyId { get; set; }
        public required string Timestamp { get; set; }
        public required string Nonce { get; set; }
        public required string Signature { get; set; }
    }

    public sealed class CocoMailEmailRequest
    {
        public string? CorrelationId { get; set; }
        public string? TemplateKey { get; set; }
        public required string To { get; set; }
        public string? CC { get; set; }
        public string? BCC { get; set; }
        public required string Subject { get; set; }
        public required string HtmlBody { get; set; }
        public int Priority { get; set; } = 5;
        public List<CocoMailAttachmentRequest> Attachments { get; set; } = [];
    }

    public sealed class CocoMailAttachmentRequest
    {
        public required string FileName { get; set; }
        public required string ContentType { get; set; }
        public long FileSize { get; set; }
        public required string StoragePath { get; set; }
    }

    public sealed class MailGatewaySendResponse
    {
        public string Code { get; set; } = string.Empty;
        public string? Message { get; set; }
        public MailGatewaySendData? Data { get; set; }
    }

    public sealed class MailGatewaySendData
    {
        public Guid EmailMessageId { get; set; }
        public string CorrelationId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
    }
}
