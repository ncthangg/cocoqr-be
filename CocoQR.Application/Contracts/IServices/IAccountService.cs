using CocoQR.Application.DTOs.Accounts.Requests;
using CocoQR.Application.DTOs.Accounts.Responses;
using CocoQR.Application.DTOs.Base.BaseRes;
using System.Runtime.CompilerServices;

namespace CocoQR.Application.Contracts.IServices
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

        Task PinAccountAsync(Guid id, bool isPinned);
        Task PatchStatusAsync(Guid id);

        Task DeleteAccountAsync(Guid id);
    }
}
