using MyWallet.Application.DTOs.Accounts.Requests;
using MyWallet.Application.DTOs.Accounts.Responses;
using MyWallet.Application.DTOs.Base.BaseRes;
using MyWallet.Domain.Constants.Enum;

namespace MyWallet.Application.Contracts.IServices
{
    public interface IAccountService
    {
        Task<PagingVM<GetAccountRes>> GetAllAsync(int pageNumber, int pageSize,
                                                           string? sortField, string? sortDirection,
                                                           Guid? userId,
                                                           Guid? providerId,
                                                           string? searchValue,
                                                           bool? isActive,
                                                           bool? isDeleted,
                                                           bool? status);
        Task<GetAccountRes> GetByIdAsync(Guid id);
        Task<Guid> PostAccountAsync(PostAccountReq request);
        Task PutAccountAsync(Guid id, PutAccountReq request);
        Task DeleteAccountAsync(Guid id);
    }
}
