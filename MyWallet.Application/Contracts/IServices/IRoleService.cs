using MyWallet.Application.DTOs.Roles.Requests;
using MyWallet.Application.DTOs.Roles.Responses;

namespace MyWallet.Application.Contracts.IServices
{
    public interface IRoleService
    {
        Task<IEnumerable<GetRoleRes>> GetAllAsync();
        Task PutAsync(Guid id, PutRoleReq req);
    }
}
