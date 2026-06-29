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
