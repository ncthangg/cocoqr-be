using MyWallet.Domain.Entities;

namespace MyWallet.Domain.Interface.IRepositories
{
    public interface IQRHistoryRepository
    {
        Task<IEnumerable<QRHistory>> GetByAccountIdAsync(Guid accountId, int pageSize = 20);
        Task<IEnumerable<QRHistory>> GetByUserIdAsync(Guid userId, DateTime fromDate, DateTime toDate);
        Task<decimal> GetTotalQRAmountAsync(Guid accountId);
    }
}
