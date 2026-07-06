using CocoQR.Application.Contracts.IRepositories.Base;
using CocoQR.Domain.Entities;

namespace CocoQR.Application.Contracts.IRepositories
{
    public interface IEmailConversationMessageRepository : IRepository<EmailConversationMessage>
    {
        Task<int> AddToConversationAsync(EmailConversationMessage message);
        Task<IEnumerable<EmailConversationMessage>> GetByConversationIdAsync(Guid conversationId);
    }
}
