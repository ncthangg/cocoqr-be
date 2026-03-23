using MyWallet.Application.DTOs.Base.BaseRes;
using MyWallet.Application.DTOs.Users.Responses;

namespace MyWallet.Application.Contracts.IServices
{
    public interface IUserService
    {
        Task<PagingVM<GetUserBaseRes>> GetUsersAsync(int pageNumber, int pageSize, string? sortField, string? sortDirection, bool? status, string? searchValue, Guid? roleId);
        Task<GetUserBySystemRes> GetByIdAsync(Guid id);
        Task PutStatusAsync(Guid id);
    }
}
