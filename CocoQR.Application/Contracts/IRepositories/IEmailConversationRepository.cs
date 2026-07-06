using CocoQR.Application.Contracts.IRepositories.Base;
using CocoQR.Domain.Entities;

namespace CocoQR.Application.Contracts.IRepositories
{
    public interface IEmailConversationRepository : IRepository<EmailConversation>
    {
        Task<EmailConversation?> GetByContactMessageIdAsync(Guid contactMessageId);
        Task UpdateLastMessageAtAsync(Guid conversationId, DateTime lastMessageAt);
    }
}
