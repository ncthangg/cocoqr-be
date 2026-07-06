using CocoQR.Application.Contracts.IRepositories;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Domain.Entities;
using CocoQR.Infrastructure.Persistence.Repositories.Base;

namespace CocoQR.Infrastructure.Persistence.Repositories
{
    public class EmailConversationRepository
        : BaseRepository<EmailConversation>, IEmailConversationRepository
    {
        public EmailConversationRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "EmailConversations")
        {
        }

        public async Task<EmailConversation?> GetByContactMessageIdAsync(Guid contactMessageId)
        {
            const string sql = """
                SELECT TOP 1 *
                FROM EmailConversations
                WHERE ContactMessageId = @ContactMessageId;
                """;

            return await QueryFirstOrDefaultAsync<EmailConversation>(
                sql,
                new { ContactMessageId = contactMessageId });
        }

        public async Task UpdateLastMessageAtAsync(Guid conversationId, DateTime lastMessageAt)
        {
            const string sql = """
                UPDATE EmailConversations
                SET LastMessageAt = @LastMessageAt
                WHERE Id = @ConversationId;
                """;

            await ExecuteAsync(sql, new
            {
                ConversationId = conversationId,
                LastMessageAt = lastMessageAt
            });
        }
    }
}
