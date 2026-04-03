using CocoQR.Application.Contracts.IRepositories.Base;
using CocoQR.Domain.Constants.Enum;
using CocoQR.Domain.Entities;

namespace CocoQR.Application.Contracts.IRepositories
{
    public interface ISmtpSettingRepository : IRepository<SmtpSetting>
    {
        Task<SmtpSetting?> GetActiveAsync(SmtpSettingType type = SmtpSettingType.Support);
        Task<SmtpSetting?> GetByTypeAsync(SmtpSettingType type);
        Task<IEnumerable<SmtpSetting>> GetAsync(SmtpSettingType? type = null);
    }
}
