using Microsoft.EntityFrameworkCore;
using MyWallet.Application.Contracts.IRepositories;
using MyWallet.Application.Contracts.IUnitOfWork;
using MyWallet.Domain.Constants.Enum;
using MyWallet.Domain.Entities;
using MyWallet.Infrastructure.Persistence.MyDbContext;
using MyWallet.Infrastructure.Persistence.Repositories.Base;

namespace MyWallet.Infrastructure.Persistence.Repositories
{
    public class QRStyleLibraryRepository : BaseRepository<QRStyleLibrary>, IQRStyleLibraryRepository
    {
        private readonly MyWalletDbContext _db;

        public QRStyleLibraryRepository(IUnitOfWork _unitOfWork, MyWalletDbContext db)
            : base(_unitOfWork, "QRStyleLibraries")
        {
            _db = db;
        }

        public async Task<IEnumerable<QRStyleLibrary>> GetAllAsync(Guid? userId, QRStyleType? type, bool? isActive, bool isAdmin)
        {
            string sql;
            if (isAdmin)
            {
                sql = @"
        SELECT 
            Id, UserId, Name, StyleJson, IsDefault, 
            Type, IsActive, CreatedAt
        FROM QRStyleLibraries
        WHERE 
            Type = 'SYSTEM'
            AND (@IsActive IS NULL OR IsActive = @IsActive)
            AND (@Type IS NULL OR Type = @Type)
        ORDER BY 
             CASE WHEN IsDefault = 1 THEN 0 ELSE 1 END,
             CreatedAt DESC
        ";
            }
            else
            {
                sql = @"
        SELECT 
            Id, UserId, Name, StyleJson, IsDefault, 
            Type, IsActive, CreatedAt
        FROM QRStyleLibraries
        WHERE 
        (
            (Type = 'SYSTEM' AND IsActive = 1)

            OR 

            (Type = 'USER' AND UserId = @UserId)
        )
        AND (@Type IS NULL OR Type = @Type)
        ORDER BY 
             CASE WHEN IsDefault = 1 THEN 0 ELSE 1 END,
             CreatedAt DESC
        ";
            }


            return await QueryAsync<QRStyleLibrary>(sql, new
            {
                UserId = userId,
                Type = type?.ToString(),
                IsActive = isActive,
            });
        }

        public override async Task AddAsync(QRStyleLibrary entity)
        {
            if (entity.IsDefault)
            {
                if (entity.Type == QRStyleType.USER)
                {
                    await ExecuteAsync(
                        @"UPDATE QRStyleLibraries
                          SET IsDefault = 0
                          WHERE Type = @Type
                            AND UserId = @UserId
                            AND IsDefault = 1",
                        new { Type = entity.Type.ToString(), entity.UserId });
                }
                else
                {
                    await ExecuteAsync(
                        @"UPDATE QRStyleLibraries
                          SET IsDefault = 0
                          WHERE Type = @Type
                            AND UserId IS NULL
                            AND IsDefault = 1",
                        new { Type = entity.Type.ToString() });
                }
            }

            await base.AddAsync(entity);
        }

        public override async Task UpdateAsync(QRStyleLibrary entity)
        {
            if (entity.IsDefault)
            {
                if (entity.Type == QRStyleType.USER)
                {
                    await ExecuteAsync(
                        @"UPDATE QRStyleLibraries
                          SET IsDefault = 0
                          WHERE Id <> @Id
                            AND Type = @Type
                            AND UserId = @UserId
                            AND IsDefault = 1",
                        new { entity.Id, Type = entity.Type.ToString(), entity.UserId });
                }
                else
                {
                    await ExecuteAsync(
                        @"UPDATE QRStyleLibraries
                          SET IsDefault = 0
                          WHERE Id <> @Id
                            AND Type = @Type
                            AND UserId IS NULL
                            AND IsDefault = 1",
                        new { entity.Id, Type = entity.Type.ToString() });
                }
            }

            await base.UpdateAsync(entity);
        }
    }
}
