using MyWallet.Domain.Entities;
using MyWallet.Domain.Interface.IRepositories.Base;

namespace MyWallet.Domain.Interface.IRepositories
{
    public interface IBankInfoRepository : IRepository<BankInfo>
    {
        Task<(IEnumerable<BankInfo>, int totalCount)> GetBankInfosAsync(int pageNumber, int pageSize, string? sortField, string? sortDirection, bool? isActive, string? searchValue);

    }
}
