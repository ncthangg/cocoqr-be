using CocoQR.Application.DTOs.Settings;

namespace CocoQR.Application.Contracts.IServices
{
    public interface ISmtpSettingService
    {
        Task<IEnumerable<GetSmtpSettingRes>> GetAsync(GetSmtpSettingReq request);
        Task<GetSmtpSettingRes> PutAsync(PutSmtpSettingReq request);
        Task DeleteAsync(Guid id);
        Task TestAsync(TestSmtpSettingReq request);
    }
}
