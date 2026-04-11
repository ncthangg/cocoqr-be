using CocoQR.Domain.Constants.Enum;

namespace CocoQR.Application.DTOs.Contacts.Responses
{
    public class GetContactMessageRes
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public ContactMessageStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RepliedAt { get; set; }
    }
}