using CocoQR.Application.Contracts.IRepositories.Base;
using CocoQR.Application.DTOs.Accounts.Queries;
using CocoQR.Domain.Entities;

namespace CocoQR.Application.Contracts.IRepositories
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
