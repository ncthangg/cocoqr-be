using MyWallet.Application.DTOs.Providers.Requests;
using MyWallet.Application.DTOs.Providers.Responses;

namespace MyWallet.Application.Contracts.IServices
{
    public interface IProviderService
    {
        Task<IEnumerable<GetProviderRes>> GetAllAsync();
        Task<GetProviderRes> GetByIdAsync(Guid id);
        Task PutAsync(Guid id, PutProviderReq req);
    }
}
