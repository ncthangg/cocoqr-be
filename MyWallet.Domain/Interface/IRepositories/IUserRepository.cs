using MyWallet.Domain.Entities;
using MyWallet.Domain.Interface.IRepositories.Base;

namespace MyWallet.Domain.Interface.IRepositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<(IEnumerable<User>, int totalCount)> GetUsersAsync(int pageNumber, int pageSize, string? sortField, string? sortDirection, bool? status, string? searchValue, Guid? roleId);
        Task<User> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<User> GetWithAccountsAsync(Guid id);

        Task<List<User>> GetUsersByIdsAsync(IEnumerable<Guid> userIds);
    }
}
