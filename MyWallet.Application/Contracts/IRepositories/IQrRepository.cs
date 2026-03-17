using MyWallet.Application.Contracts.IRepositories.Base;
using MyWallet.Domain.Constants.Enum;
using MyWallet.Domain.Entities;

namespace MyWallet.Application.Contracts.IRepositories
{
    public interface IQrRepository : IRepository<QRHistory>
    {
        Task<IEnumerable<QRHistory>> GetByAccountIdAsync(Guid accountId,
                                                         int pageNumber, int pageSize,
                                                         string? sortField, string? sortDirection,
                                                         Guid? providerId,
                                                         QRReceiverType? receiverType,
                                                         bool? isFixedAmount, bool? isPaid,
                                                         string? searchValue);
        Task<IEnumerable<QRHistory>> GetByUserIdAsync(Guid userId,
                                                      int pageNumber, int pageSize,
                                                      DateTime fromDate,
                                                      DateTime toDate);
        Task<decimal> GetTotalQRAmountAsync(Guid accountId,
                                            bool? isPaid,
                                            Guid? providerId,
                                            QRReceiverType? receiverType);


        Task<long> Post(QRHistory req);
    }
}
