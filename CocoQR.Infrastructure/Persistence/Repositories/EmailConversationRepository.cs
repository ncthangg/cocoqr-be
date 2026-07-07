using CocoQR.Application.Contracts.IRepositories;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Application.DTOs.Contacts.Queries;
using CocoQR.Domain.Constants.Enum;
using CocoQR.Domain.Entities;
using CocoQR.Infrastructure.Persistence.Repositories.Base;
using Dapper;

namespace CocoQR.Infrastructure.Persistence.Repositories
{
    public class EmailConversationRepository
        : BaseRepository<EmailConversation>, IEmailConversationRepository
    {
        public EmailConversationRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "EmailConversations")
        {
        }

        public async Task<(IEnumerable<ContactConversationQueryDto> Items, int TotalCount)> GetPagedForAdminAsync(
            int pageNumber,
            int pageSize,
            string? sortField,
            string? sortDirection,
            Guid? userId,
            string? searchValue,
            ContactMessageStatus? contactStatus,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var orderBy = "c.LastMessageAt DESC";

            if (!string.IsNullOrEmpty(sortField))
            {
                var dir = sortDirection?.ToUpperInvariant() == "ASC" ? "ASC" : "DESC";

                orderBy = sortField switch
                {
                    "createdAt" => $"c.CreatedAt {dir}",
                    "lastMessageAt" => $"c.LastMessageAt {dir}",
                    "subject" => $"c.Subject {dir}",
                    _ => "c.LastMessageAt DESC"
                };
            }

            var sql = $@"
                SELECT
                    CASE
                        WHEN c.ContactMessageId IS NULL THEN c.Id
                        ELSE c.ContactMessageId
                    END AS Id,
                    c.Id AS ConversationId,
                    c.ContactMessageId,
                    COALESCE(cm.FullName, c.RecipientEmail, c.InitiatorEmail) AS FullName,
                    COALESCE(cm.Email, c.RecipientEmail, c.InitiatorEmail) AS Email,
                    COALESCE(cm.Content, latest.Body, '') AS Content,
                    c.Subject,
                    cm.Status,
                    COALESCE(cm.CreatedAt, c.CreatedAt) AS CreatedAt,
                    c.LastMessageAt,
                    cm.RepliedAt
                FROM EmailConversations c
                LEFT JOIN ContactMessages cm ON cm.Id = c.ContactMessageId
                OUTER APPLY
                (
                    SELECT TOP 1 Body
                    FROM EmailConversationMessages m
                    WHERE m.ConversationId = c.Id
                    ORDER BY m.SequenceNumber DESC
                ) latest
                WHERE (@UserId IS NULL OR c.InitiatorUserId = @UserId OR c.RecipientUserId = @UserId)
                  AND (@ContactStatus IS NULL OR cm.Status = @ContactStatus)
                  AND (@FromDate IS NULL OR c.CreatedAt >= @FromDate)
                  AND (@ToDateExclusive IS NULL OR c.CreatedAt < @ToDateExclusive)
                  AND (
                      @SearchValue IS NULL
                      OR c.Subject LIKE @SearchPattern
                      OR c.InitiatorEmail LIKE @SearchPattern
                      OR c.RecipientEmail LIKE @SearchPattern
                      OR cm.FullName LIKE @SearchPattern
                      OR cm.Email LIKE @SearchPattern
                      OR cm.Content LIKE @SearchPattern
                      OR latest.Body LIKE @SearchPattern
                  )
                ORDER BY {orderBy}
                OFFSET (@PageNumber - 1) * @PageSize ROWS
                FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(1)
                FROM EmailConversations c
                LEFT JOIN ContactMessages cm ON cm.Id = c.ContactMessageId
                OUTER APPLY
                (
                    SELECT TOP 1 Body
                    FROM EmailConversationMessages m
                    WHERE m.ConversationId = c.Id
                    ORDER BY m.SequenceNumber DESC
                ) latest
                WHERE (@UserId IS NULL OR c.InitiatorUserId = @UserId OR c.RecipientUserId = @UserId)
                  AND (@ContactStatus IS NULL OR cm.Status = @ContactStatus)
                  AND (@FromDate IS NULL OR c.CreatedAt >= @FromDate)
                  AND (@ToDateExclusive IS NULL OR c.CreatedAt < @ToDateExclusive)
                  AND (
                      @SearchValue IS NULL
                      OR c.Subject LIKE @SearchPattern
                      OR c.InitiatorEmail LIKE @SearchPattern
                      OR c.RecipientEmail LIKE @SearchPattern
                      OR cm.FullName LIKE @SearchPattern
                      OR cm.Email LIKE @SearchPattern
                      OR cm.Content LIKE @SearchPattern
                      OR latest.Body LIKE @SearchPattern
                  );
            ";

            DateTime? toDateExclusive = null;
            if (toDate.HasValue)
            {
                toDateExclusive = toDate.Value.Date.AddDays(1);
            }

            var trimmedSearch = string.IsNullOrWhiteSpace(searchValue)
                ? null
                : searchValue.Trim();

            using var multi = await _unitOfWork.Connection.QueryMultipleAsync(
                sql,
                new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    UserId = userId,
                    SearchValue = trimmedSearch,
                    SearchPattern = trimmedSearch == null ? null : $"%{trimmedSearch}%",
                    ContactStatus = contactStatus?.ToString(),
                    FromDate = fromDate,
                    ToDateExclusive = toDateExclusive
                },
                _unitOfWork.Transaction);

            var items = await multi.ReadAsync<ContactConversationQueryDto>();
            var totalCount = await multi.ReadSingleAsync<int>();

            return (items, totalCount);
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
