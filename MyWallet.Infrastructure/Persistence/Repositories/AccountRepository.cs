using MyWallet.Domain.Entities;
using MyWallet.Domain.Interface.IRepositories;
using MyWallet.Infrastructure.Persistence.Repositories.Base;
using IDbConnectionFactory = MyWallet.Domain.Interface.IDbContext.IDbConnectionFactory;

namespace MyWallet.Infrastructure.Persistence.Repositories
{
    public class AccountRepository : BaseRepository<Account>, IAccountRepository
    {
        public AccountRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "Accounts")
        {

        }

        public async Task<(IEnumerable<Account>, int totalCount)> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize, bool? isActive)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID", nameof(userId));

            const string sql = @"
        SELECT 
            Id, UserId, AccountNumber, AccountHolder, 
            BankCode, BankName, AccountType, Balance, IsActive
            Status, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedAt, DeletedBy
        FROM Accounts
        WHERE 
            (@IsActive IS NULL OR IsActive = @IsActive)
            AND (@UserId == UserId)
        ORDER BY BankName ASC
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;

        SELECT COUNT(1)
        FROM Accounts
        WHERE 
            (@IsActive IS NULL OR IsActive = @IsActive)
            AND (@UserId == UserId);
    ";

            return await QueryPagedAsync<Account>(sql, new
            {
                UserId = userId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                IsActive = isActive,
            });
        }

        public async Task<Account> GetByAccountNumberAsync(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
                throw new ArgumentException("Account number cannot be empty", nameof(accountNumber));

            const string sql = "SELECT * FROM Accounts WHERE AccountNumber = @AccountNumber";

            return await QuerySingleAsync<Account>(sql, new { AccountNumber = accountNumber });
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

            var count = await QuerySingleAsync<int>(
                sql,
                new { UserId = userId, AccountNumber = accountNumber, ExcludeId = excludeAccountId }
            );

            return count > 0;
        }
    }
}
