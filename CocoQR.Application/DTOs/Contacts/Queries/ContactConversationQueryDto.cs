using CocoQR.Domain.Constants.Enum;

namespace CocoQR.Application.DTOs.Contacts.Queries
{
    public class ContactConversationQueryDto
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Guid? ContactMessageId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public ContactMessageStatus? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastMessageAt { get; set; }
        public DateTime? RepliedAt { get; set; }
    }
}
