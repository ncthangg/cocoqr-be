using CocoQR.Application.Contracts.IRepositories.Base;
using CocoQR.Domain.Entities;

namespace CocoQR.Application.Contracts.IRepositories
{
    public interface IRoleRepository : IRepository<Role>
    {
        Task<Role?> GetByNameAsync(string roleName);
    }
}
