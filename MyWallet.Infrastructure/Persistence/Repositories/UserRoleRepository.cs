using Dapper;
using MyWallet.Domain.Entities;
using MyWallet.Domain.Interface.IRepositories;
using MyWallet.Domain.Interface.IUnitOfWork;
using MyWallet.Infrastructure.Persistence.Repositories.Base;

namespace MyWallet.Infrastructure.Persistence.Repositories
{
    public class UserRoleRepository : BaseRepository<UserRole>, IUserRoleRepository
    {
        public UserRoleRepository(IUnitOfWork _unitOfWork)
                       : base(_unitOfWork, "UserRoles")
        {
        }
        public async Task<(IEnumerable<UserRole>, int totalCount)> GetAllUserRolesAsync(int pageNumber, int pageSize, Guid? roleId)
        {
            const string sql = @"
        SELECT 
            ur.UserId,
            ur.RoleId,
            ur.CreatedAt,
            ur.Status,

            r.Id,
            r.Name,
            r.NameUpperCase

        FROM UserRoles ur
        INNER JOIN Roles r ON ur.RoleId = r.Id
        WHERE (@RoleId IS NULL OR ur.RoleId = @RoleId)
        ORDER BY ur.CreatedAt DESC
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY

        SELECT COUNT(1)
        FROM UserRoles
        WHERE 
            (@RoleId IS NULL OR RoleId = @RoleId);
    ";

            //return await QueryPagedAsync<GetUserRoleRes>(sql, new
            //{
            //    PageNumber = pageNumber,
            //    PageSize = pageSize,
            //    RoleId = roleId
            //});

            var multi = await _unitOfWork.Connection.QueryMultipleAsync(sql,
                new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    RoleId = roleId
                },
                _unitOfWork.Transaction
            );

            var items = multi.Read<UserRole, Role, UserRole>(
                (ur, role) =>
                {
                    ur.Role = role;
                    return ur;
                },
                splitOn: "Id"
            );

            var totalCount = multi.ReadSingle<int>();

            return (items, totalCount);
        }
        public async Task<IEnumerable<Role>> GetRolesByUserIdAsync(Guid userId)
        {
            const string sql = @"
            SELECT r.Id, r.Name, r.NameUpperCase
            FROM UserRoles ur
            INNER JOIN Roles r ON ur.RoleId = r.Id
            WHERE ur.UserId = @UserId AND ur.Status = 1
        ";

            return await QueryAsync<Role>(sql,
                new
                {
                    UserId = userId
                }
            );
        }
        public async Task<int> AddUserToRoleAsync(Guid id, Guid userId, Guid roleId)
        {
            const string sql = @"
            INSERT INTO UserRoles (Id, UserId, RoleId, CreatedAt, Status)
            VALUES (@Id, @UserId, @RoleId, GETUTCDATE(), 1)
        ";

            return await ExecuteAsync(sql,
                new
                {
                    Id = id,
                    UserId = userId,
                    RoleId = roleId
                }
            );
        }
        public async Task<int> RemoveUserFromRoleAsync(Guid userId, Guid roleId)
        {
            const string sql = @"
            DELETE FROM UserRoles
            WHERE UserId = @UserId AND RoleId = @RoleId
        ";

            return await ExecuteAsync(sql,
                new
                {
                    UserId = userId,
                    RoleId = roleId
                }
            );
        }
        public async Task<bool> ExistsAsync(Guid userId, Guid roleId)
        {
            const string sql = @"
        SELECT COUNT(1)
        FROM UserRoles
        WHERE UserId = @UserId AND RoleId = @RoleId
        ";

            var count = await QueryFirstOrDefaultAsync<int>(sql,
                new
                {
                    userId,
                    roleId
                }
            );

            return count > 0;
        }
    }
}
