using CocoQR.Application.Contracts.IRepositories;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Domain.Entities;
using CocoQR.Infrastructure.Persistence.Repositories.Base;

namespace CocoQR.Infrastructure.Persistence.Repositories
{
    public class EmailConversationMessageRepository
        : BaseRepository<EmailConversationMessage>, IEmailConversationMessageRepository
    {
        public EmailConversationMessageRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "EmailConversationMessages")
        {
        }

        public async Task<int> AddToConversationAsync(EmailConversationMessage message)
        {
            const string sql = """
                INSERT INTO EmailConversationMessages
                (
                    Id,
                    ConversationId,
                    SequenceNumber,
                    SenderUserId,
                    RecipientUserId,
                    FromEmail,
                    ToEmail,
                    Subject,
                    Body,
                    Direction,
                    Status,
                    GatewayMessageId,
                    ErrorMessage,
                    CreatedAt
                )
                OUTPUT inserted.SequenceNumber
                SELECT
                    @Id,
                    @ConversationId,
                    COALESCE(MAX(SequenceNumber), 0) + 1,
                    @SenderUserId,
                    @RecipientUserId,
                    @FromEmail,
                    @ToEmail,
                    @Subject,
                    @Body,
                    @Direction,
                    @Status,
                    @GatewayMessageId,
                    @ErrorMessage,
                    @CreatedAt
                FROM EmailConversationMessages WITH (UPDLOCK, HOLDLOCK)
                WHERE ConversationId = @ConversationId;
                """;

            return await QuerySingleAsync<int>(sql, new
            {
                message.Id,
                message.ConversationId,
                message.SenderUserId,
                message.RecipientUserId,
                message.FromEmail,
                message.ToEmail,
                message.Subject,
                message.Body,
                Direction = message.Direction.ToString(),
                Status = message.Status.ToString(),
                message.GatewayMessageId,
                message.ErrorMessage,
                message.CreatedAt
            });
        }

        public Task<IEnumerable<EmailConversationMessage>> GetByConversationIdAsync(
            Guid conversationId)
        {
            const string sql = """
                SELECT
                    Id,
                    ConversationId,
                    SequenceNumber,
                    SenderUserId,
                    RecipientUserId,
                    FromEmail,
                    ToEmail,
                    Subject,
                    Body,
                    Direction,
                    Status,
                    GatewayMessageId,
                    ErrorMessage,
                    CreatedAt
                FROM EmailConversationMessages
                WHERE ConversationId = @ConversationId
                ORDER BY SequenceNumber ASC;
                """;

            return QueryAsync<EmailConversationMessage>(sql, new { ConversationId = conversationId });
        }
    }
}
