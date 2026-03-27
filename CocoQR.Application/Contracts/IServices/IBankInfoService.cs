using CocoQR.Application.DTOs.Banks.Requests;
using CocoQR.Application.DTOs.Banks.Responses;
using CocoQR.Application.DTOs.Base.BaseRes;

namespace CocoQR.Application.Contracts.IServices
{
    public interface IBankInfoService
    {
        Task<PagingVM<GetBankInfoRes>> GetsAsync(int pageNumber, int pageSize, string? sortField, string? sortDirection, bool? isActive, string? searchValue);
        Task<GetBankInfoRes> GetByIdAsync(Guid id);
        Task PutAsync(Guid id, PutBankInfoReq req);
    }
}
