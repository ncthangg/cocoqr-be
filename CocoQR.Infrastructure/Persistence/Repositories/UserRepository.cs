using Dapper;
using CocoQR.Application.Contracts.IRepositories;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Application.DTOs.Users.Responses;
using CocoQR.Domain.Entities;
using CocoQR.Infrastructure.Persistence.Repositories.Base;

namespace CocoQR.Infrastructure.Persistence.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(IUnitOfWork _unitOfWork)
            : base(_unitOfWork, "Users")
        {
        }
        public async Task<(IEnumerable<GetUserBaseRes>, int totalCount)> GetUsersAsync(int pageNumber,
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
            u.PictureUrl,
            u.CreatedAt,
            u.UpdatedAt,
            u.Status
        FROM Users u
        WHERE
            (@Status IS NULL OR u.Status = @Status)
            AND (
                @SearchValue IS NULL
                OR u.FullName LIKE '%' + @SearchValue + '%'
                OR u.Email LIKE @SearchValue + '%'
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
                OR u.Email LIKE @SearchValue + '%'
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
                return ([], totalCount);

            #region Query roles

            var userIds = users.Select(x => x.Id).ToList();

            var sqlRoles = @"
        SELECT 
            ur.UserId,
            ur.RoleId,
            r.Name,
            r.NameUpperCase
        FROM UserRoles ur
        INNER JOIN Roles r ON r.Id = ur.RoleId
        WHERE ur.UserId IN @UserIds
        ";

            var roles = await _unitOfWork.Connection.QueryAsync<UserRoleRaw>(
                           sqlRoles,
                           new { UserIds = userIds },
                           transaction: _unitOfWork.Transaction
                       );

            #endregion

            #region Map roles to users

            var roleLookup = roles
                            .GroupBy(x => x.UserId)
                            .ToDictionary(
                                g => g.Key,
                                g => g.Select(r => new UserRoleRaw
                                {
                                    RoleId = r.RoleId,
                                    Name = r.Name,
                                    NameUpperCase = r.NameUpperCase
                                }).ToList()
                            );

            var result = users.Select(u => new GetUserBaseRes
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                PictureUrl = u.PictureUrl,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                Status = u.Status,
                Roles = roleLookup.TryGetValue(u.Id, out var roles)
        ? roles
        : []
            }).ToList();

            #endregion

            return (result, totalCount);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            const string sql = @"
                SELECT 
                    Id, Email, FullName, GoogleId, SecurityStamp, PictureUrl, CreatedAt, UpdatedAt, Status
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

            const string sql = @"SELECT CASE
                WHEN EXISTS (
                     SELECT 1
                     FROM Users 
                     WHERE Email = @Email
                )
                THEN 1 ELSE 0 END
            ";

            return await QuerySingleAsync<bool>(sql,
                new
                {
                    Email = email
                }
            );
        }
    }
}
