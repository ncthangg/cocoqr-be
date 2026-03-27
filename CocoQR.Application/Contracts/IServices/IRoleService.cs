using CocoQR.Application.DTOs.Roles.Requests;
using CocoQR.Application.DTOs.Roles.Responses;

namespace CocoQR.Application.Contracts.IServices
{
    public interface IRoleService
    {
        Task<IEnumerable<GetRoleRes>> GetAllAsync();
        Task PutAsync(Guid id, PutRoleReq req);
    }
}
