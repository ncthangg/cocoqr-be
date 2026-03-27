using CocoQR.Application.Contracts.IRepositories.Base;
using CocoQR.Application.DTOs.Providers.Queries;
using CocoQR.Domain.Entities;

namespace CocoQR.Application.Contracts.IRepositories
{
    public interface IProviderRepository : IRepository<Provider>
    {
        Task<IEnumerable<ProviderQueryDto>> GetAllAsync(bool isAdmin);
    }
}
