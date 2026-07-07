using CocoQR.Application.DTOs.Contacts.Responses;
using CocoQR.Application.DTOs.Contacts.Queries;
using CocoQR.Domain.Entities;

namespace CocoQR.Application.Common.Mapper
{
    public static class ContactMapper
    {
        public static GetContactMessageRes ToResponse(ContactMessage message)
        {
            return new GetContactMessageRes
            {
                Id = message.Id,
                ContactMessageId = message.Id,
                FullName = message.FullName,
                Email = message.Email,
                Content = message.Content,
                Status = message.Status,
                CreatedAt = message.CreatedAt,
                LastMessageAt = message.RepliedAt ?? message.CreatedAt,
                RepliedAt = message.RepliedAt
            };
        }

        public static GetContactMessageRes ToResponse(ContactConversationQueryDto conversation)
        {
            return new GetContactMessageRes
            {
                Id = conversation.Id,
                ConversationId = conversation.ConversationId,
                ContactMessageId = conversation.ContactMessageId,
                FullName = conversation.FullName,
                Email = conversation.Email,
                Content = conversation.Content,
                Subject = conversation.Subject,
                Status = conversation.Status,
                CreatedAt = conversation.CreatedAt,
                LastMessageAt = conversation.LastMessageAt,
                RepliedAt = conversation.RepliedAt
            };
        }
    }
}
