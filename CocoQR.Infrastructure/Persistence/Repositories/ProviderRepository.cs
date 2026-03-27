using CocoQR.Application.Contracts.IRepositories;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Application.DTOs.Providers.Queries;
using CocoQR.Domain.Entities;
using CocoQR.Infrastructure.Persistence.Repositories.Base;

namespace CocoQR.Infrastructure.Persistence.Repositories
{
    public class ProviderRepository : BaseRepository<Provider>, IProviderRepository
    {
        public ProviderRepository(IUnitOfWork _unitOfWork)
                       : base(_unitOfWork, "Providers")
        {
        }
        public async Task<IEnumerable<ProviderQueryDto>> GetAllAsync(bool isAdmin)
        {
            string sql;

            if (isAdmin)
            {
                sql = $@"SELECT
                     p.Id, p.Code, p.Name, p.IsActive, p.LogoUrl,
                     p.Status,

                     p.CreatedBy,
                     p.UpdatedBy,
                     p.DeletedBy,

                     u1.FullName AS CreatedByName,
                     u2.FullName AS UpdatedByName,
                     u3.FullName AS DeletedByName,

                     p.CreatedAt,
                     p.UpdatedAt,
                     p.DeletedAt
                FROM Providers p
                    LEFT JOIN Users u1 ON p.CreatedBy = u1.Id
                    LEFT JOIN Users u2 ON p.UpdatedBy = u2.Id
                    LEFT JOIN Users u3 ON p.DeletedBy = u3.Id
                ";
            }
            else
            {
                sql = $@"SELECT
                     Id, Code, Name, IsActive, LogoUrl
                FROM Providers
                WHERE DeletedAt IS NULL
                     AND Status = 1
                ";
            }

            return await QueryAsync<ProviderQueryDto>(sql, null);
        }
    }
}
