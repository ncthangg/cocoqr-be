using CocoQR.Application.Contracts.IRepositories.Base;
using CocoQR.Domain.Entities;

namespace CocoQR.Application.Contracts.IRepositories
{
    public interface IQRStyleRepository : IRepository<QRStyle>
    {
        Task<QRStyle?> GetByQrIdAsync(long qrId);
    }
}
