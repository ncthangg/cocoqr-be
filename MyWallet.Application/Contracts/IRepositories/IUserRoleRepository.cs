using MyWallet.Application.Contracts.IRepositories.Base;
using MyWallet.Domain.Entities;

namespace MyWallet.Application.Contracts.IRepositories
{
    public interface IUserRoleRepository : IRepository<UserRole>
    {
        Task<IEnumerable<Role>> GetRolesByUserIdAsync(Guid userId);
        Task<int> AddUserToRoleAsync(Guid id, Guid userId, Guid roleId);
        Task<int> RemoveUserFromRoleAsync(Guid userId, IEnumerable<Guid> roleIds);
        Task<bool> ExistsAsync(Guid userId, Guid roleId);
    }
}
