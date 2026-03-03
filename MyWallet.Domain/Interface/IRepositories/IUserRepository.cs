using MyWallet.Domain.Entities;
using MyWallet.Domain.Interface.IRepositories.Base;

namespace MyWallet.Domain.Interface.IRepositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<User> GetWithAccountsAsync(Guid id);

        Task<List<User>> GetUsersByIdsAsync(IEnumerable<Guid> userIds);
    }
}
