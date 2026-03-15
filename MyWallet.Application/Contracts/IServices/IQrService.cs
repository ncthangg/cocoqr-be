using MyWallet.Application.DTOs.QR.Requests;
using MyWallet.Application.DTOs.QR.Responses;

namespace MyWallet.Application.Contracts.IServices
{
    public interface IQrService
    {
        Task<GetQrRes> CreateAsync(PostQrReq request);
    }
}
