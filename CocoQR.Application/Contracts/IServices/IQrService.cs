using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Application.DTOs.QR.Requests;
using CocoQR.Application.DTOs.QR.Responses;

namespace CocoQR.Application.Contracts.IServices
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
