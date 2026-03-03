using MyWallet.Domain.Entities;
using MyWallet.Domain.Interface.IRepositories.Base;

namespace MyWallet.Domain.Interface.IRepositories
{
    public interface IAccountRepository : IRepository<Account>
    {
        Task<(IEnumerable<Account>, int totalCount)> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize, bool? isActive);
        Task<Account> GetByAccountNumberAsync(string accountNumber);
        Task<bool> AccountNumberExistsAsync(Guid userId, string accountNumber, Guid? excludeAccountId = null);
    }
}
