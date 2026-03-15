using MyWallet.Application.Contracts.IRepositories.Base;
using MyWallet.Application.DTOs.Providers.NewFolder;
using MyWallet.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWallet.Application.Contracts.IRepositories
{
    public interface IProviderRepository : IRepository<Provider>
    {
        Task<IEnumerable<ProviderQueryDto>> GetAllAsync(bool isAdmin);
    }
}
