using CocoQR.Application.Contracts.IRepositories;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Domain.Constants.Enum;
using CocoQR.Domain.Entities;
using CocoQR.Infrastructure.Persistence.Repositories.Base;

namespace CocoQR.Infrastructure.Persistence.Repositories
{
    public class SmtpSettingRepository : BaseRepository<SmtpSetting>, ISmtpSettingRepository
    {
        public SmtpSettingRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "SmtpSettings")
        {
        }

        public async Task<SmtpSetting?> GetActiveAsync(SmtpSettingType type = SmtpSettingType.Support)
        {
            const string sql = @"
                SELECT TOP 1
                    Id,
                    Host,
                    Port,
                    Username,
                    Password,
                    EnableSSL,
                    FromEmail,
                    FromName,
                    Type,
                    IsActive,
                    UpdatedAt
                FROM SmtpSettings
                WHERE IsActive = 1
                  AND Type = @Type
                ORDER BY UpdatedAt DESC
            ";

            return await QueryFirstOrDefaultAsync<SmtpSetting>(sql, new { Type = type.ToString() });
        }

        public async Task<SmtpSetting?> GetByTypeAsync(SmtpSettingType type)
        {
            const string sql = @"
                SELECT TOP 1
                    Id,
                    Host,
                    Port,
                    Username,
                    Password,
                    EnableSSL,
                    FromEmail,
                    FromName,
                    Type,
                    IsActive,
                    UpdatedAt
                FROM SmtpSettings
                WHERE Type = @Type
                ORDER BY UpdatedAt DESC
            ";

            return await QueryFirstOrDefaultAsync<SmtpSetting>(sql, new { Type = type.ToString() });
        }

        public async Task<IEnumerable<SmtpSetting>> GetAsync(SmtpSettingType? type = null)
        {
            const string sql = @"
                SELECT
                    Id,
                    Host,
                    Port,
                    Username,
                    Password,
                    EnableSSL,
                    FromEmail,
                    FromName,
                    Type,
                    IsActive,
                    UpdatedAt
                FROM SmtpSettings
                WHERE (@Type IS NULL OR Type = @Type)
                ORDER BY Type ASC, UpdatedAt DESC
            ";

            return await QueryAsync<SmtpSetting>(sql, new { Type = type?.ToString() });
        }
    }
}
