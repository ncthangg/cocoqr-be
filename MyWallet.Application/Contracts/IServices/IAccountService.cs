using MyWallet.Application.DTOs.Request;
using MyWallet.Application.DTOs.Response;
using MyWallet.Application.DTOs.Response.Base;

namespace MyWallet.Application.Contracts.IServices
{
    public interface IAccountService
    {
        Task<PagingVM<GetAccountRes>> GetUserAccountsAsync(Guid userId, int pageNumber, int pageSize, string? sortField, string? sortDirection, bool? isActive, string? searchValue);
        Task<GetAccountRes> GetByIdAsync(Guid id);
        Task<Guid> PostAccountAsync(PostAccountReq request);
        Task PutAccountAsync(Guid id, PutAccountReq request);
        Task DeleteAccountAsync(Guid id);
    }
}
