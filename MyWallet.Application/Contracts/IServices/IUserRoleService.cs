using MyWallet.Application.DTOs.Request;
using MyWallet.Application.DTOs.Response;
using MyWallet.Application.DTOs.Response.Base;

namespace MyWallet.Application.Contracts.IServices
{
    public interface IUserRoleService
    {
        Task<IEnumerable<GetRoleRes>> GetRolesByUserIdAsync(Guid userId);
        Task<bool> PostPutUserRolesAsync(PostPutUserRoleReq req);
    }
}
