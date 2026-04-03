using CocoQR.Application.DTOs.Contacts.Responses;
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
                FullName = message.FullName,
                Email = message.Email,
                Content = message.Content,
                Status = message.Status,
                CreatedAt = message.CreatedAt,
                RepliedAt = message.RepliedAt
            };
        }
    }
}
