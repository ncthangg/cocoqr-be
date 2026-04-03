using CocoQR.Application.Contracts.IRepositories.Base;
using CocoQR.Application.DTOs.EmailLogs.Queries;
using CocoQR.Domain.Constants.Enum;
using CocoQR.Domain.Entities;

namespace CocoQR.Application.Contracts.IRepositories
{
    public interface IEmailLogRepository : IRepository<EmailLog>
    {
        Task<(IEnumerable<EmailLogListQueryDto>, int totalCount)> GetAsync(
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
            DateTime? toDateExclusive);

        Task<EmailLogQueryDto?> GetByIdDetailAsync(Guid id);
    }
}