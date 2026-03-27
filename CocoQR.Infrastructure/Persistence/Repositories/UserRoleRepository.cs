using CocoQR.Application.Contracts.IRepositories;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Domain.Entities;
using CocoQR.Infrastructure.Persistence.Repositories.Base;

namespace CocoQR.Infrastructure.Persistence.Repositories
{
    public class UserRoleRepository : BaseRepository<UserRole>, IUserRoleRepository
    {
        public UserRoleRepository(IUnitOfWork _unitOfWork)
                       : base(_unitOfWork, "UserRoles")
        {
        }
        public async Task<IEnumerable<Role>> GetRolesByUserIdAsync(Guid userId)
        {
            const string sql = @"
            SELECT r.Id, r.Name, r.NameUpperCase
            FROM UserRoles ur
            INNER JOIN Roles r ON ur.RoleId = r.Id
            WHERE ur.UserId = @UserId
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
        public async Task<int> RemoveUserFromRoleAsync(Guid userId, IEnumerable<Guid> roleIds)
        {
            const string sql = @"
            DELETE FROM UserRoles
            WHERE UserId = @UserId
            AND RoleId IN @RoleIds
            ";

            return await ExecuteAsync(sql, new
            {
                UserId = userId,
                RoleIds = roleIds
            });
        }
        public async Task<bool> ExistsAsync(Guid userId, Guid roleId)
        {
            const string sql = @"
            SELECT CASE
            WHEN EXISTS (
                SELECT 1
                FROM UserRoles
                WHERE UserId = @UserId AND RoleId = @RoleId
            )
            THEN 1 ELSE 0 END
            ";

            return await QuerySingleAsync<bool>(sql,
                new
                {
                    userId,
                    roleId
                }
            );
        }
    }
}
