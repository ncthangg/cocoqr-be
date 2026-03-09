using MyWallet.Domain.Entities;
using MyWallet.Domain.Interface.IRepositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWallet.Domain.Interface.IRepositories
{
    public interface IUserRoleRepository : IRepository<UserRole>
    {
        Task<IEnumerable<Role>> GetRolesByUserIdAsync(Guid userId);
        Task<int> AddUserToRoleAsync(Guid id, Guid userId, Guid roleId);
        Task<int> RemoveUserFromRoleAsync(Guid userId, IEnumerable<Guid> roleIds);
        Task<bool> ExistsAsync(Guid userId, Guid roleId);
    }
}
