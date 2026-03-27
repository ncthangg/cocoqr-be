using CocoQR.Application.DTOs.QRStyleLibrary.Requests;
using CocoQR.Application.DTOs.QRStyleLibrary.Responses;
using CocoQR.Domain.Constants.Enum;

namespace CocoQR.Application.Contracts.IServices
{
    public interface IQrStyleLibraryService
    {
        Task<IEnumerable<GetQrStyleLibraryRes>> GetAllAsync(QRStyleType? type, bool? isActive);

        Task<Guid> PostUserStyleAsync(PostQRStyleReq request);
        Task PutUserStyleAsync(Guid styleId, PutQRStyleReq request);
        Task DeleteUserStyleAsync(Guid styleId);
    }
}
