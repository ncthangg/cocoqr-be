using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Application.DTOs.EmailLogs.Requests;
using CocoQR.Application.DTOs.EmailLogs.Responses;

namespace CocoQR.Application.Contracts.IServices
{
    public interface IEmailLogService
    {
        Task<PagingVM<GetEmailLogRes>> GetAsync(GetEmailLogReq request);
        Task<GetEmailLogByIdRes> GetByIdAsync(Guid id);
    }
}