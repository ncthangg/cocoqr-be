using MyWallet.Application.DTOs.Base.BaseRes;
using MyWallet.Application.DTOs.QR.Requests;
using MyWallet.Application.DTOs.QR.Responses;

namespace MyWallet.Application.Contracts.IServices
{
    public interface IQrService
    {
        Task<PagingVM<GetQrRes>> GetAllAsync(int pageNumber, int pageSize,
                                             string? sortField, string? sortDirection,
                                             Guid? userId,
                                             Guid? providerId,
                                             string? searchValue,
                                             bool? isDeleted,
                                             bool? status);
        Task<GetQrRes> GetByIdAsync(long id);
        Task<PostQrRes> GenerateAsync(PostQrReq request);
        Task<PostQrRes> RegenerateImageAsync(Guid qrHistoryId);
    }
}
