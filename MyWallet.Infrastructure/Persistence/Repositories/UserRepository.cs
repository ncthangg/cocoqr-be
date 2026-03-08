using Dapper;
using MyWallet.Domain.Entities;
using MyWallet.Domain.Interface.IRepositories;
using MyWallet.Domain.Interface.IUnitOfWork;
using MyWallet.Infrastructure.Persistence.Repositories.Base;

namespace MyWallet.Infrastructure.Persistence.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(IUnitOfWork _unitOfWork)
            : base(_unitOfWork, "Users")
        {
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            const string sql = @"
                SELECT 
                    Id, Email, FullName, GoogleId, SecurityStamp, PictureUrl, CreatedAt, UpdatedAt
                FROM Users
                WHERE Email = @Email
            ";

            return await QueryFirstOrDefaultAsync<User>(sql,
                new
                {
                    Email = email
                }
            );
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            const string sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email";

            var count = await QueryFirstOrDefaultAsync<int>(sql,
                new
                {
                    Email = email
                }
            );
            return count > 0;
        }

        public async Task<User> GetWithAccountsAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Invalid user ID", nameof(id));

            const string sql = "SELECT * FROM Users WHERE Id = @UserId";

            return await QueryFirstOrDefaultAsync<User>(sql,
                new
                {
                    UserId = id
                }
            );
        }
        public async Task<List<User>> GetUsersByIdsAsync(IEnumerable<Guid> userIds)
        {
            const string sql = "SELECT Id, FullName FROM Users WHERE Id IN @Ids";

            return await QueryFirstOrDefaultAsync<List<User>>(sql,
                new
                {
                    Ids = userIds
                }
            );
        }
    }
}
