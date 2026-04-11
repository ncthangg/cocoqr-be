using CocoQR.Application.Contracts.IRepositories;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Domain.Constants.Enum;
using CocoQR.Domain.Entities;
using CocoQR.Infrastructure.Persistence.Repositories.Base;
using Dapper;

namespace CocoQR.Infrastructure.Persistence.Repositories
{
    public class ContactMessageRepository : BaseRepository<ContactMessage>, IContactMessageRepository
    {
        public ContactMessageRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "ContactMessages")
        {
        }

        public async Task<(IEnumerable<ContactMessage> Items, int TotalCount)> GetPagedForAdminAsync(
            int pageNumber,
            int pageSize,
            string? sortField,
            string? sortDirection,
            Guid? userId,
            Guid? providerId,
            string? searchValue,
            bool? isActive,
            ContactMessageStatus? contactStatus,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var orderBy = "CreatedAt DESC";

            if (!string.IsNullOrEmpty(sortField))
            {
                var dir = sortDirection?.ToUpper() == "DESC" ? "DESC" : "ASC";

                orderBy = sortField switch
                {
                    "createdAt" => $"createdAt {dir}",
                    _ => "CreatedAt DESC"
                };
            }

            var sql = $@"
                SELECT
                    Id,
                    FullName,
                    Email,
                    Status,
                    CreatedAt,
                    RepliedAt
                FROM ContactMessages
                WHERE (@ContactStatus IS NULL OR Status = @ContactStatus)
                  AND (@FromDate IS NULL OR CreatedAt >= @FromDate)
                  AND (@ToDateExclusive IS NULL OR CreatedAt < @ToDateExclusive)
                ORDER BY {orderBy}

                OFFSET (@PageNumber - 1) * @PageSize ROWS
                FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(1)
                FROM ContactMessages
                WHERE (@ContactStatus IS NULL OR Status = @ContactStatus)
                  AND (@FromDate IS NULL OR CreatedAt >= @FromDate)
                  AND (@ToDateExclusive IS NULL OR CreatedAt < @ToDateExclusive)
            ";

            DateTime? toDateExclusive = null;
            if (toDate.HasValue)
            {
                toDateExclusive = toDate.Value.Date.AddDays(1);
            }

            using var multi = await _unitOfWork.Connection.QueryMultipleAsync(
                sql,
                new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    ContactStatus = contactStatus?.ToString(),
                    FromDate = fromDate,
                    ToDateExclusive = toDateExclusive
                },
                _unitOfWork.Transaction);

            var items = await multi.ReadAsync<ContactMessage>();
            var totalCount = await multi.ReadSingleAsync<int>();

            return (items, totalCount);
        }

        public async Task<ContactMessage?> GetByIdForAdminAsync(Guid id)
        {
            const string sql = @"
                SELECT TOP 1
                    Id,
                    FullName,
                    Email,
                    Content,
                    Status,
                    CreatedAt,
                    RepliedAt
                FROM ContactMessages
                WHERE Id = @Id
            ";

            return await QueryFirstOrDefaultAsync<ContactMessage>(sql, new { Id = id });
        }
    }
}