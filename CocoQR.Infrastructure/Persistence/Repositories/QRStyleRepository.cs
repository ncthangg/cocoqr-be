using CocoQR.Application.Contracts.IRepositories;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Domain.Entities;
using CocoQR.Infrastructure.Persistence.Repositories.Base;

namespace CocoQR.Infrastructure.Persistence.Repositories
{
    public class QRStyleRepository : BaseRepository<QRStyle>, IQRStyleRepository
    {
        public QRStyleRepository(IUnitOfWork _unitOfWork)
            : base(_unitOfWork, "QRStyles")
        {
        }

        public async Task<QRStyle?> GetByQrIdAsync(long qrId)
        {
            var sql = @"SELECT TOP 1 * FROM QRStyles WHERE QrId = @QrId ORDER BY CreatedAt DESC";
            return await QueryFirstOrDefaultAsync<QRStyle>(sql, new { QrId = qrId });
        }
    }
}
