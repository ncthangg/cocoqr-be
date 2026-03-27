using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Application.DTOs.Users.Responses;

namespace CocoQR.Application.Contracts.IServices
{
    public interface IUserService
    {
        Task<PagingVM<GetUserBaseRes>> GetUsersAsync(int pageNumber, int pageSize, string? sortField, string? sortDirection, bool? status, string? searchValue, Guid? roleId);
        Task<GetUserBySystemRes> GetByIdAsync(Guid id);
        Task PutStatusAsync(Guid id);
    }
}
