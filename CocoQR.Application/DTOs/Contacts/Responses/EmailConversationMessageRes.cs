using CocoQR.Domain.Constants.Enum;

namespace CocoQR.Application.DTOs.Contacts.Responses
{
    public class EmailConversationMessageRes
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public int SequenceNumber { get; set; }
        public Guid? SenderUserId { get; set; }
        public Guid? RecipientUserId { get; set; }
        public string FromEmail { get; set; } = string.Empty;
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public EmailDirection Direction { get; set; }
        public EmailDeliveryStatus Status { get; set; }
        public Guid? GatewayMessageId { get; set; }
        public string? CorrelationId { get; set; }
        public string? LastCallbackEventId { get; set; }
        public DateTime? LastCallbackAt { get; set; }
        public string? FailureCode { get; set; }
        public string? ProviderMessageId { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
