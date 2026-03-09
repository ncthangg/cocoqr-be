using Dapper;
using MyWallet.Domain.Entities;
using MyWallet.Domain.Interface.IRepositories;
using MyWallet.Domain.Interface.IUnitOfWork;
using MyWallet.Infrastructure.Persistence.Repositories.Base;

namespace MyWallet.Infrastructure.Persistence.Repositories
{
    public class AccountRepository : BaseRepository<Account>, IAccountRepository
    {
        public AccountRepository(IUnitOfWork _unitOfWork)
            : base(_unitOfWork, "Accounts")
        {
        }

        public async Task<(IEnumerable<Account>, int totalCount)> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize, string? sortField, string? sortDirection, bool? isActive, string? searchValue)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID", nameof(userId));

            var orderBy = "CreatedAt DESC";

            if (!string.IsNullOrEmpty(sortField))
            {
                var dir = sortDirection?.ToUpper() == "DESC" ? "DESC" : "ASC";

                orderBy = sortField switch
                {
                    "accountNumber" => $"AccountNumber {dir}",
                    "accountHolder" => $"AccountHolder {dir}",
                    "bankCode" => $"BankCode {dir}",
                    "bankName" => $"BankName {dir}",
                    _ => "CreatedAt DESC"
                };
            }

            var sql = $@"
        SELECT 
            Id, UserId, AccountNumber, AccountHolder, 
            BankCode, BankName, AccountType, Balance, IsActive
            Status, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedAt, DeletedBy
        FROM Accounts
        WHERE 
            (@IsActive IS NULL OR IsActive = @IsActive)
            AND (@UserId == UserId)
            AND (
                @SearchValue IS NULL 
                OR AccountNumber LIKE '%' + @SearchValue + '%'
                OR AccountHolder LIKE '%' + @SearchValue + '%'
                OR BankCode LIKE '%' + @SearchValue + '%'
                OR BankName LIKE '%' + @SearchValue + '%'
            )
        ORDER BY {orderBy}
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;

        SELECT COUNT(1)
        FROM Accounts
        WHERE 
            (@IsActive IS NULL OR IsActive = @IsActive)
            AND (@UserId == UserId)
            AND (
                @SearchValue IS NULL 
                OR AccountNumber LIKE '%' + @SearchValue + '%'
                OR AccountHolder LIKE '%' + @SearchValue + '%'
                OR BankCode LIKE '%' + @SearchValue + '%'
                OR BankName LIKE '%' + @SearchValue + '%'
            );
        ";

            return await QueryPagedAsync<Account>(sql,
                new
                {
                    UserId = userId,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    IsActive = isActive,
                }
            );
        }

        public async Task<Account> GetByAccountNumberAsync(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
                throw new ArgumentException("Account number cannot be empty", nameof(accountNumber));

            const string sql = "SELECT * FROM Accounts WHERE AccountNumber = @AccountNumber";

            return await QueryFirstOrDefaultAsync<Account>(sql,
                new
                {
                    AccountNumber = accountNumber
                }
            );
        }

        public async Task<bool> AccountNumberExistsAsync(Guid userId, string accountNumber, Guid? excludeAccountId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID", nameof(userId));
            if (string.IsNullOrWhiteSpace(accountNumber))
                throw new ArgumentException("Account number cannot be empty", nameof(accountNumber));

            const string sql = @"
                SELECT 1
                FROM Accounts
                WHERE UserId = @UserId
                  AND AccountNumber = @AccountNumber
                  AND (@ExcludeId IS NULL OR Id <> @ExcludeId)
            ";

            var count = await QueryFirstOrDefaultAsync<int>(sql,
                new
                {
                    UserId = userId,
                    AccountNumber = accountNumber,
                    ExcludeId = excludeAccountId
                }
            );

            return count > 0;
        }
    }
}
