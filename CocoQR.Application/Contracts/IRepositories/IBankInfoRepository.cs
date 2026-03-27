using CocoQR.Application.Contracts.IRepositories.Base;
using CocoQR.Domain.Entities;

namespace CocoQR.Application.Contracts.IRepositories
{
    public interface IBankInfoRepository : IRepository<BankInfo>
    {
        Task<(IEnumerable<BankInfo>, int totalCount)> GetBankInfosAsync(int pageNumber, int pageSize, string? sortField, string? sortDirection, bool? isActive, string? searchValue, bool isAdmin);

        Task<BankInfo> GetByBankCodeAsync(string bankCode);
    }
}
