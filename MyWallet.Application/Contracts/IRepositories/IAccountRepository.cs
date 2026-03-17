using MyWallet.Application.Contracts.IRepositories.Base;
using MyWallet.Application.DTOs.Accounts.Queries;
using MyWallet.Domain.Entities;

namespace MyWallet.Application.Contracts.IRepositories
{
    public interface IAccountRepository : IRepository<Account>
    {
        Task<(IEnumerable<AccountQueryDto>, int totalCount)> GetAllAsync(int pageNumber, int pageSize,
                                                                         string? sortField, string? sortDirection,
                                                                         Guid? userId,
                                                                         Guid? providerId,
                                                                         string? searchValue,
                                                                         bool? isActive,
                                                                         bool? isDeleted,
                                                                         bool? status);
        Task<AccountQueryDto?> GetByIdAsync(Guid id, Guid? userId, bool isAdmin);
        Task<bool> AccountNumberExistsAsync(Guid userId, string accountNumber, Guid providerId, string? bankCode, Guid? excludeAccountId = null);
    }
}
