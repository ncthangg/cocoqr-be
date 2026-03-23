using MyWallet.Application.DTOs.Roles.Responses;
using MyWallet.Application.DTOs.UserRoles.Requests;

namespace MyWallet.Application.Contracts.IServices
{
    public interface IUserRoleService
    {
        Task<IEnumerable<GetRoleRes>> GetRolesByUserIdAsync(Guid userId);
        Task<bool> PostPutUserRolesAsync(PostPutUserRoleReq req);
    }
}
