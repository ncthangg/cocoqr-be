using MyWallet.Application.DTOs.Response;
using MyWallet.Application.DTOs.Response.Base;

namespace MyWallet.Application.Contracts.IServices
{
    public interface IUserService
    {
        Task<PagingVM<GetUserBaseRes>> GetUsersAsync(int pageNumber, int pageSize, string? sortField, string? sortDirection, bool? status, string? searchValue, Guid? roleId);
    }
}
