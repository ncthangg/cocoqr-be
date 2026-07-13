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
                    CorrelationId,
                    LastCallbackEventId,
                    LastCallbackAt,
                    LastCallbackPayload,
                    FailureCode,
                    ProviderMessageId,
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
                    @CorrelationId,
                    @LastCallbackEventId,
                    @LastCallbackAt,
                    @LastCallbackPayload,
                    @FailureCode,
                    @ProviderMessageId,
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
                message.CorrelationId,
                message.LastCallbackEventId,
                message.LastCallbackAt,
                message.LastCallbackPayload,
                message.FailureCode,
                message.ProviderMessageId,
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
                    CorrelationId,
                    LastCallbackEventId,
                    LastCallbackAt,
                    LastCallbackPayload,
                    FailureCode,
                    ProviderMessageId,
                    ErrorMessage,
                    CreatedAt
                FROM EmailConversationMessages
                WHERE ConversationId = @ConversationId
                ORDER BY SequenceNumber ASC;
                """;

            return QueryAsync<EmailConversationMessage>(sql, new { ConversationId = conversationId });
        }

        public async Task<EmailConversationMessage?> GetByGatewayMessageIdAsync(Guid gatewayMessageId)
        {
            const string sql = """
                SELECT TOP 1
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
                    CorrelationId,
                    LastCallbackEventId,
                    LastCallbackAt,
                    LastCallbackPayload,
                    FailureCode,
                    ProviderMessageId,
                    ErrorMessage,
                    CreatedAt
                FROM EmailConversationMessages
                WHERE GatewayMessageId = @GatewayMessageId;
                """;

            var message = await QueryFirstOrDefaultAsync<EmailConversationMessage>(
                sql,
                new { GatewayMessageId = gatewayMessageId });
            return message;
        }

        public async Task UpdateDeliveryStatusAsync(EmailConversationMessage message)
        {
            const string sql = """
                UPDATE EmailConversationMessages
                SET
                    Status = @Status,
                    LastCallbackEventId = @LastCallbackEventId,
                    LastCallbackAt = @LastCallbackAt,
                    LastCallbackPayload = @LastCallbackPayload,
                    FailureCode = @FailureCode,
                    ProviderMessageId = @ProviderMessageId,
                    ErrorMessage = @ErrorMessage
                WHERE Id = @Id;
                """;

            await ExecuteAsync(sql, new
            {
                message.Id,
                Status = message.Status.ToString(),
                message.LastCallbackEventId,
                message.LastCallbackAt,
                message.LastCallbackPayload,
                message.FailureCode,
                message.ProviderMessageId,
                message.ErrorMessage
            });
        }
    }
}
