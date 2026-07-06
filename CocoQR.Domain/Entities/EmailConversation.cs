namespace CocoQR.Domain.Entities
{
    public class EmailConversation
    {
        public Guid Id { get; set; }
        public Guid? ContactMessageId { get; set; }
        public Guid? InitiatorUserId { get; set; }
        public Guid? RecipientUserId { get; set; }
        public string InitiatorEmail { get; set; } = string.Empty;
        public string RecipientEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public DateTime LastMessageAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
