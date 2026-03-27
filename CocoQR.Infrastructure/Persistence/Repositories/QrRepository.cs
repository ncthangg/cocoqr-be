using CocoQR.Application.Contracts.IRepositories;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Application.DTOs.QR.Queries;
using CocoQR.Domain.Constants.Enum;
using CocoQR.Domain.Entities;
using CocoQR.Infrastructure.Persistence.Repositories.Base;

namespace CocoQR.Infrastructure.Persistence.Repositories
{
    public class QrRepository : BaseRepository<QRHistory>, IQrRepository
    {
        public QrRepository(IUnitOfWork _unitOfWork)
            : base(_unitOfWork, "QRHistories")
        {
        }
        public async Task<(IEnumerable<QrHistoryQueryDto>, int totalCount)> GetAllAsync(int pageNumber, int pageSize,
                                                                           string? sortField, string? sortDirection,
                                                                           Guid? userId,
                                                                           Guid? providerId,
                                                                           string? searchValue,
                                                                           bool? isDeleted,
                                                                           bool? status)
        {
            var orderBy = "CreatedAt DESC";

            if (!string.IsNullOrEmpty(sortField))
            {
                var dir = sortDirection?.ToUpper() == "DESC" ? "DESC" : "ASC";

                orderBy = sortField switch
                {
                    "accountHolderSnapshot" => $"AccountHolderSnapshot {dir}",
                    "createdAt" => $"CreatedAt {dir}",
                    _ => "CreatedAt DESC"
                };
            }

            var sql = $@"
        SELECT
            q.Id, q.UserId, u.Email, q.AccountId,
            q.AccountNumberSnapshot, q.AccountHolderSnapshot,
            q.BankCodeSnapshot, q.NapasBinSnapshot, q.BankNameSnapshot, q.BankShortNameSnapshot,
            q.ProviderId, p.Code AS ProviderCode, p.Name AS ProviderName, p.LogoUrl AS ProviderLogoUrl,
            q.ReceiverType, q.QrMode, q.Status AS QrStatus,

            q.CreatedAt
        FROM QRHistories q
            LEFT JOIN Providers p
                 ON q.ProviderId = p.Id
            LEFT JOIN Users u
                 ON q.UserId = u.Id
        WHERE
            (@UserId IS NULL OR q.UserId = @UserId)
            AND (
                 @IsDeleted IS NULL
                 OR (@IsDeleted = 1 AND q.DeletedAt IS NOT NULL)
                 OR (@IsDeleted = 0 AND q.DeletedAt IS NULL)
            )
            AND (@ProviderId IS NULL OR q.ProviderId = @ProviderId)
            AND (
                @SearchValue IS NULL
                OR q.AccountNumberSnapshot LIKE '%' + @SearchValue + '%'
                OR ISNULL(q.AccountHolderSnapshot,'') LIKE '%' + @SearchValue + '%'
                OR ISNULL(q.BankCodeSnapshot,'')  LIKE '%' + @SearchValue + '%'
                OR ISNULL(q.NapasBinSnapshot,'')  LIKE '%' + @SearchValue + '%'
                OR u.Email LIKE @SearchValue + '%'
            )
        ORDER BY 
            {orderBy}
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;

        SELECT COUNT(1)
        FROM QRHistories q
            LEFT JOIN Providers p
                 ON q.ProviderId = p.Id
            LEFT JOIN Users u
                 ON q.UserId = u.Id
        WHERE
            (@UserId IS NULL OR q.UserId = @UserId)
            AND (
                 @IsDeleted IS NULL
                 OR (@IsDeleted = 1 AND q.DeletedAt IS NOT NULL)
                 OR (@IsDeleted = 0 AND q.DeletedAt IS NULL)
            )
            AND (@ProviderId IS NULL OR q.ProviderId = @ProviderId)
            AND (
                @SearchValue IS NULL
                OR q.AccountNumberSnapshot LIKE '%' + @SearchValue + '%'
                OR ISNULL(q.AccountHolderSnapshot,'') LIKE '%' + @SearchValue + '%'
                OR ISNULL(q.BankCodeSnapshot,'')  LIKE '%' + @SearchValue + '%'
                OR ISNULL(q.NapasBinSnapshot,'')  LIKE '%' + @SearchValue + '%'
                OR u.Email LIKE @SearchValue + '%'
            );
        ";

            return await QueryPagedAsync<QrHistoryQueryDto>(sql,
                new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    UserId = userId,
                    ProviderId = providerId,
                    SearchValue = searchValue,
                    IsDeleted = isDeleted,
                    Status = status,
                    //FromDtate = "",
                    //ToDate = ""
                }
            );
        }
        public async Task<QrHistoryQueryDto?> GetByIdAsync(long id, Guid? userId, bool isAdmin)
        {
            string sql;

            if (isAdmin)
            {
                sql = $@"SELECT
                q.Id, q.UserId, u.Email, q.AccountId,
                q.AccountNumberSnapshot, q.AccountHolderSnapshot,
                q.BankCodeSnapshot, q.NapasBinSnapshot, q.BankNameSnapshot, q.BankShortNameSnapshot,
                q.Amount, q.Currency, q.Description,
                q.QrData, q.TransactionRef,
                q.ProviderId, p.Code AS ProviderCode, p.Name AS ProviderName, p.LogoUrl AS ProviderLogoUrl,
                q.ReceiverType, q.IsFixedAmount, q.QrMode, q.Status AS QrStatus,

                q.CreatedAt, q.ExpiredAt, q.PaidAt, q.DeletedAt
                FROM QRHistories q
                LEFT JOIN Providers p
                     ON q.ProviderId = p.Id
                LEFT JOIN Users u
                     ON q.UserId = u.Id
                WHERE q.Id = @Id";
            }
            else
            {
                sql = $@"SELECT
                q.Id, q.UserId, q.AccountId,
                q.AccountNumberSnapshot, q.AccountHolderSnapshot,
                q.BankCodeSnapshot, q.NapasBinSnapshot, q.BankNameSnapshot, q.BankShortNameSnapshot,
                q.Amount, q.Currency, q.Description,
                q.QrData, q.TransactionRef,
                q.ProviderId, p.Code AS ProviderCode, p.Name AS ProviderName, p.LogoUrl AS ProviderLogoUrl,
                q.ReceiverType, q.IsFixedAmount, q.QrMode, q.Status AS QrStatus,

                q.CreatedAt, q.ExpiredAt, q.PaidAt
                FROM QRHistories q
                    LEFT JOIN Providers p
                         ON q.ProviderId = p.Id
                WHERE q.Id = @Id
                     AND q.UserId = @UserId
                     AND q.DeletedAt IS NULL
                ";
            }

            return await QuerySingleAsync<QrHistoryQueryDto>(sql,
                new
                {
                    Id = id,
                    UserId = userId
                });
        }
        public async Task<IEnumerable<QRHistory>> GetByAccountIdAsync(Guid accountId,
            int pageNumber, int pageSize,
            string? sortField, string? sortDirection,
            Guid? providerId,
            QRReceiverType? receiverType,
            bool? isFixedAmount, bool? isPaid,
            string? searchValue)
        {
            if (accountId == Guid.Empty)
                throw new ArgumentException("Invalid account ID", nameof(accountId));

            var orderBy = "CreatedAt DESC";

            if (!string.IsNullOrEmpty(sortField))
            {
                var dir = sortDirection?.ToUpper() == "DESC" ? "DESC" : "ASC";

                orderBy = sortField switch
                {
                    "accountNumberSnapshot" => $"AccountNumberSnapshot {dir}",
                    "accountHolderSnapshot" => $"AccountHolderSnapshot {dir}",
                    "bankCodeSnapshot" => $"BankCodeSnapshot {dir}",
                    "bankNameSnapshot" => $"BankNameSnapshot {dir}",
                    "description" => $"Description {dir}",
                    _ => "CreatedAt DESC"
                };
            }

            var sql = $@"
                SELECT
                    Id, UserId, AccountId,
                    AccountNumberSnapshot, AccountHolderSnapshot,
                    BankCodeSnapshot, NapasBinSnapshot, BankNameSnapshot, BankShortNameSnapshot,
                    Amount, Description, ProviderCode, ReceiverType,
                    IsFixedAmount, IsPaid,
                    CreatedAt, ExpiredAt, PaidAt, DeletedAt
                FROM QRHistories
                WHERE
                    (AccountId = @AccountId)
                    AND (@ProviderCode IS NULL OR ProviderCode = @ProviderCode)
                    AND (@ReceiverType IS NULL OR ReceiverType = @ReceiverType)
                    AND (@IsFixedAmount IS NULL OR IsFixedAmount = @IsFixedAmount)
                    AND (@IsPaid IS NULL OR IsPaid = @IsPaid)
                    AND (
                        @SearchValue IS NULL
                        OR ISNULL(AccountNumberSnapshot,'') LIKE '%' + @SearchValue + '%'
                        OR ISNULL(AccountHolderSnapshot,'') LIKE '%' + @SearchValue + '%'
                        OR ISNULL(BankCodeSnapshot,'') LIKE '%' + @SearchValue + '%'
                        OR ISNULL(BankNameSnapshot,'') LIKE '%' + @SearchValue + '%'
                        OR ISNULL(Description,'') LIKE '%' + @SearchValue + '%'
                    )
                ORDER BY {orderBy}
                OFFSET (@PageNumber - 1) * @PageSize ROWS
                FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(1)
                FROM QRHistories
                WHERE
                    (AccountId = @AccountId)
                    AND (@ProviderCode IS NULL OR ProviderCode = @ProviderCode)
                    AND (@ReceiverType IS NULL OR ReceiverType = @ReceiverType)
                    AND (@IsFixedAmount IS NULL OR IsFixedAmount = @IsFixedAmount)
                    AND (@IsPaid IS NULL OR IsPaid = @IsPaid)
                    AND (
                        @SearchValue IS NULL
                        OR ISNULL(AccountNumberSnapshot,'') LIKE '%' + @SearchValue + '%'
                        OR ISNULL(AccountHolderSnapshot,'') LIKE '%' + @SearchValue + '%'
                        OR ISNULL(BankCodeSnapshot,'') LIKE '%' + @SearchValue + '%'
                        OR ISNULL(BankNameSnapshot,'') LIKE '%' + @SearchValue + '%'
                        OR ISNULL(Description,'') LIKE '%' + @SearchValue + '%'
                    )
                ";

            return await QueryAsync<QRHistory>(sql,
                new
                {
                    AccountId = accountId,
                    Provider = providerId,
                    ReceiverType = receiverType?.ToString(),
                    IsFixedAmount = isFixedAmount,
                    IsPaid = isPaid,
                    SearchValue = searchValue,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                }
            );
        }

        public async Task<IEnumerable<QRHistory>> GetByUserIdAsync(
            Guid userId,
            int pageNumber, int pageSize,
            DateTime fromDate,
            DateTime toDate)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID", nameof(userId));
            if (fromDate > toDate)
                throw new ArgumentException("FromDate must be before ToDate");

            const string sql = @"
                SELECT 
                    Id, UserId, AccountId,
                    AccountNumberSnapshot, AccountHolderSnapshot,
                    BankCodeSnapshot, NapasBinSnapshot, BankNameSnapshot, BankShortNameSnapshot,
                    Amount, Description, QrData, ProviderCode, ReceiverType,
                    IsFixedAmount, IsPaid,
                    CreatedAt, ExpiredAt, PaidAt, DeletedAt
                FROM QRHistories
                WHERE 
                      UserId = @UserId 
                      AND CreatedAt >= @FromDate
                      AND CreatedAt < DATEADD(DAY,1,@ToDate)
                ORDER BY CreatedAt DESC
                OFFSET (@PageNumber - 1) * @PageSize ROWS
                FETCH NEXT @PageSize ROWS ONLY;
            ";

            return await QueryAsync<QRHistory>(sql,
                new
                {
                    UserId = userId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                }
            );
        }

        public async Task<decimal> GetTotalQRAmountAsync(Guid accountId,
                                                         bool? isPaid,
                                                         Guid? providerId,
                                                         QRReceiverType? receiverType)
        {
            if (accountId == Guid.Empty)
                throw new ArgumentException("Invalid account ID", nameof(accountId));

            const string sql = @"
                SELECT COALESCE(SUM(Amount), 0)
                FROM QRHistories
                WHERE 
                     AccountId = @AccountId
                     AND (@IsPaid IS NULL OR IsPaid = @IsPaid)
                     AND (@ProviderId IS NULL OR ProviderId = @ProviderId)
                     AND (@ReceiverType IS NULL OR ReceiverType = @ReceiverType)
            ";

            return await QueryFirstOrDefaultAsync<decimal>(sql,
                new
                {
                    AccountId = accountId,
                    IsPaid = isPaid,
                    ProviderId = providerId,
                    ReceiverType = receiverType
                }
            );
        }

        public async Task<long> Post(QRHistory req)
        {
            const string sql = @"
                INSERT INTO QRHistories
                (
                    UserId, AccountId,
                    AccountNumberSnapshot, AccountHolderSnapshot,
                    BankCodeSnapshot, NapasBinSnapshot, BankNameSnapshot, BankShortNameSnapshot,
                    Amount, Currency, Description,
                    QrData, TransactionRef,
                    ProviderId, ReceiverType, IsFixedAmount, QrMode,
                    CreatedAt
                )
                VALUES
                (
                    @UserId, @AccountId,
                    @AccountNumberSnapshot, @AccountHolderSnapshot,
                    @BankCodeSnapshot, @NapasBinSnapshot, @BankNameSnapshot, @BankShortNameSnapshot,
                    @Amount, @Currency, @Description,
                    @QrData, @TransactionRef,
                    @ProviderId, @ReceiverType, @IsFixedAmount, @QrMode,
                    @CreatedAt
                );
                
                SELECT CAST(SCOPE_IDENTITY() as BIGINT);
            ";

            return await QuerySingleAsync<long>(sql, new
            {
                req.UserId,
                req.AccountId,

                req.AccountNumberSnapshot,
                req.AccountHolderSnapshot,

                req.BankCodeSnapshot,
                req.NapasBinSnapshot,
                req.BankNameSnapshot,
                req.BankShortNameSnapshot,

                req.Amount,
                Currency = req.Currency.ToString(),
                req.Description,

                req.QrData,
                req.TransactionRef,

                req.ProviderId,
                ReceiverType = req.ReceiverType.ToString(),
                req.IsFixedAmount,
                QrMode = req.QrMode.ToString(),

                req.CreatedAt
            });
        }
    }
}
