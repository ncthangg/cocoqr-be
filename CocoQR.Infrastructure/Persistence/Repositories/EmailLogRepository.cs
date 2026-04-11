using CocoQR.Application.Contracts.IRepositories;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Application.DTOs.EmailLogs.Queries;
using CocoQR.Domain.Constants.Enum;
using CocoQR.Domain.Entities;
using CocoQR.Infrastructure.Persistence.Repositories.Base;

namespace CocoQR.Infrastructure.Persistence.Repositories
{
    public class EmailLogRepository : BaseRepository<EmailLog>, IEmailLogRepository
    {
        public EmailLogRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "EmailLogs")
        {
        }

        public async Task<(IEnumerable<EmailLogListQueryDto>, int totalCount)> GetAsync(
            int pageNumber,
            int pageSize,
            SmtpSettingType? smtpType,
            EmailLogStatus? status,
            EmailDirection? direction,
            Guid? recipientUserId,
            string? toEmail,
            string? recipientFullName,
            string? subject,
            DateTime? fromDate,
            DateTime? toDateExclusive)
        {
            const string sql = @"
                SELECT
                    el.Id,
                    u.Id AS RecipientUserId,
                    u.FullName AS RecipientFullName,
                    el.ToEmail,
                    el.Subject,
                    el.Status,
                    el.ErrorMessage,
                    el.SmtpType,
                    el.EmailDirection,
                    el.TemplateKey,
                    el.CreatedAt
                FROM EmailLogs el
                LEFT JOIN Users u ON u.Email = el.ToEmail
                WHERE
                    (@SmtpType IS NULL OR el.SmtpType = @SmtpType)
                    AND (@Status IS NULL OR el.Status = @Status)
                    AND (@Direction IS NULL OR el.EmailDirection = @Direction)
                    AND (@RecipientUserId IS NULL OR u.Id = @RecipientUserId)
                    AND (@ToEmail IS NULL OR el.ToEmail LIKE CONCAT('%', @ToEmail, '%'))
                    AND (@RecipientFullName IS NULL OR u.FullName LIKE CONCAT('%', @RecipientFullName, '%'))
                    AND (@Subject IS NULL OR el.Subject LIKE CONCAT('%', @Subject, '%'))
                    AND (@FromDate IS NULL OR el.CreatedAt >= @FromDate)
                    AND (@ToDateExclusive IS NULL OR el.CreatedAt < @ToDateExclusive)
                ORDER BY el.CreatedAt DESC
                OFFSET (@PageNumber - 1) * @PageSize ROWS
                FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(1)
                FROM EmailLogs el
                LEFT JOIN Users u ON u.Email = el.ToEmail
                WHERE
                    (@SmtpType IS NULL OR el.SmtpType = @SmtpType)
                    AND (@Status IS NULL OR el.Status = @Status)
                    AND (@Direction IS NULL OR el.EmailDirection = @Direction)
                    AND (@RecipientUserId IS NULL OR u.Id = @RecipientUserId)
                    AND (@FromDate IS NULL OR el.CreatedAt >= @FromDate)
                    AND (@ToDateExclusive IS NULL OR el.CreatedAt < @ToDateExclusive);
            ";

            return await QueryPagedAsync<EmailLogListQueryDto>(sql, new
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SmtpType = smtpType?.ToString(),
                Status = status?.ToString(),
                Direction = direction?.ToString(),
                RecipientUserId = recipientUserId,
                ToEmail = toEmail,
                RecipientFullName = recipientFullName,
                Subject = subject,
                FromDate = fromDate,
                ToDateExclusive = toDateExclusive
            });
        }

        public async Task<EmailLogQueryDto?> GetByIdDetailAsync(Guid id)
        {
            const string sql = @"
                SELECT TOP 1
                    el.Id,
                    u.Id AS RecipientUserId,
                    u.FullName AS RecipientFullName,
                    el.ToEmail,
                    el.Subject,
                    el.Body,
                    el.Status,
                    el.ErrorMessage,
                    el.SmtpType,
                    el.EmailDirection,
                    el.TemplateKey,
                    el.CreatedAt
                FROM EmailLogs el
                LEFT JOIN Users u ON u.Email = el.ToEmail
                WHERE el.Id = @Id
            ";

            return await QueryFirstOrDefaultAsync<EmailLogQueryDto>(sql, new { Id = id });
        }
    }
}