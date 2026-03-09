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
        public async Task<(IEnumerable<User>, int totalCount)> GetUsersAsync(int pageNumber,
            int pageSize,
            string? sortField,
            string? sortDirection,
            bool? status,
            string? searchValue,
            Guid? roleId)
        {
            var orderBy = "u.FullName ASC";

            if (!string.IsNullOrEmpty(sortField))
            {
                var dir = sortDirection?.ToUpper() == "DESC" ? "DESC" : "ASC";

                orderBy = sortField switch
                {
                    "fullName" => $"u.FullName {dir}",
                    "email" => $"u.Email {dir}",
                    "createdAt" => $"u.CreatedAt {dir}",
                    _ => "u.FullName ASC"
                };
            }

            #region Query users (paging)

            var sqlUsers = $@"
        SELECT 
            u.Id,
            u.FullName,
            u.Email,
            u.Status
        FROM Users u
        WHERE
            (@Status IS NULL OR u.Status = @Status)
            AND (
                @SearchValue IS NULL
                OR u.FullName LIKE '%' + @SearchValue + '%'
                OR u.Email LIKE '%' + @SearchValue + '%'
            )
            AND (
                @RoleId IS NULL
                OR EXISTS (
                    SELECT 1
                    FROM UserRoles ur
                WHERE ur.UserId = u.Id
                      AND ur.RoleId = @RoleId
                )
            )
        ORDER BY {orderBy}
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;

        SELECT COUNT(1)
        FROM Users u
        WHERE
            (@Status IS NULL OR u.Status = @Status)
            AND (
                @SearchValue IS NULL
                OR u.FullName LIKE '%' + @SearchValue + '%'
                OR u.Email LIKE '%' + @SearchValue + '%'
            )
            AND (
                @RoleId IS NULL
                OR EXISTS (
                    SELECT 1
                    FROM UserRoles ur
                WHERE ur.UserId = u.Id
                      AND ur.RoleId = @RoleId
                )
            );
        ";

            var multi = await _unitOfWork.Connection.QueryMultipleAsync(
                sqlUsers,
                new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Status = status,
                    SearchValue = searchValue,
                    RoleId = roleId
                },
                _unitOfWork.Transaction
            );

            var users = (await multi.ReadAsync<User>()).ToList();
            var totalCount = await multi.ReadSingleAsync<int>();

            #endregion

            if (!users.Any())
                return (users, totalCount);

            #region Query roles

            var userIds = users.Select(x => x.Id).ToList();

            var sqlRoles = @"
        SELECT 
            ur.UserId,
            r.Id,
            r.Name,
            r.NameUpperCase
        FROM UserRoles ur
        INNER JOIN Roles r ON r.Id = ur.RoleId
        WHERE ur.UserId IN @UserIds
        ";

            var roles = await _unitOfWork.Connection.QueryAsync<UserRole, Role, UserRole>(
                sqlRoles,
                (ur, role) =>
                {
                    ur.Role = role;
                    return ur;
                },
                new { UserIds = userIds },
                splitOn: "Id",
                transaction: _unitOfWork.Transaction
            );

            #endregion

            #region Map roles to users

            var roleLookup = roles.GroupBy(x => x.UserId)
                                  .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var user in users)
            {
                if (roleLookup.TryGetValue(user.Id, out var userRoles))
                    user.UserRoles = userRoles;
                else
                    user.UserRoles = new List<UserRole>();
            }

            #endregion

            return (users, totalCount);
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
