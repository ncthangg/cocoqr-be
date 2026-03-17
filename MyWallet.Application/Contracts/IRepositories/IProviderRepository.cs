using MyWallet.Application.Contracts.IRepositories.Base;
using MyWallet.Application.DTOs.Providers.NewFolder;
using MyWallet.Domain.Entities;

namespace MyWallet.Application.Contracts.IRepositories
{
    public interface IProviderRepository : IRepository<Provider>
    {
        Task<IEnumerable<ProviderQueryDto>> GetAllAsync(bool isAdmin);
    }
}
