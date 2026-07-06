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
        public EmailConversationDirection Direction { get; set; }
        public EmailConversationMessageStatus Status { get; set; }
        public Guid? GatewayMessageId { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
