using MyWallet.Application.DTOs.Request;
using MyWallet.Application.DTOs.Response;
using MyWallet.Application.DTOs.Response.Base;

namespace MyWallet.Application.Contracts.IServices
{
    public interface IBankInfoService
    {
        Task<PagingVM<GetBankInfoRes>> GetsAsync(int pageNumber, int pageSize, string? sortField, string? sortDirection, bool? isActive, string? searchValue);
        Task<GetBankInfoRes> GetByIdAsync(Guid id);
        Task PostAsync(PostBankInfoReq req);
        Task PutAsync(Guid id, PutBankInfoReq req);
        Task DeleteAsync(Guid id);
    }
}
