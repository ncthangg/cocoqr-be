using MyWallet.Application.Contracts.IRepositories.Base;
using MyWallet.Application.DTOs.Users.Responses;
using MyWallet.Domain.Entities;

namespace MyWallet.Application.Contracts.IRepositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<(IEnumerable<GetUserBaseRes>, int totalCount)> GetUsersAsync(int pageNumber, int pageSize, string? sortField, string? sortDirection, bool? status, string? searchValue, Guid? roleId);
        Task<User> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
    }
}
