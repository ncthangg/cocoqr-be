using CocoQR.Domain.Constants.Enum;

namespace CocoQR.Domain.Entities
{
    public class ContactMessage
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Content { get; set; } = null!;
        public ContactMessageStatus Status { get; set; } = ContactMessageStatus.NEW;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RepliedAt { get; set; }
    }
}
