using CocoQR.Application.DTOs.Providers.Requests;
using CocoQR.Application.DTOs.Providers.Responses;

namespace CocoQR.Application.Contracts.IServices
{
    public interface IProviderService
    {
        Task<IEnumerable<GetProviderRes>> GetAllAsync();
        Task<GetProviderRes> GetByIdAsync(Guid id);
        Task PutAsync(Guid id, PutProviderReq req);
    }
}
