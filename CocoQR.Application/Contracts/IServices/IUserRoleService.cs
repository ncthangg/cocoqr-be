using CocoQR.Application.DTOs.Roles.Responses;
using CocoQR.Application.DTOs.UserRoles.Requests;

namespace CocoQR.Application.Contracts.IServices
{
    public interface IUserRoleService
    {
        Task<IEnumerable<GetRoleRes>> GetRolesByUserIdAsync(Guid userId);
        Task<bool> PostPutUserRolesAsync(PostPutUserRoleReq req);
    }
}
