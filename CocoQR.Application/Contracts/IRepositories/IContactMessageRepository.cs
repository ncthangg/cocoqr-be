using CocoQR.Application.Contracts.IRepositories.Base;
using CocoQR.Domain.Constants.Enum;
using CocoQR.Domain.Entities;

namespace CocoQR.Application.Contracts.IRepositories
{
    public interface IContactMessageRepository : IRepository<ContactMessage>
    {
        Task<(IEnumerable<ContactMessage> Items, int TotalCount)> GetPagedForAdminAsync(
            int pageNumber,
            int pageSize,
            string? sortField,
            string? sortDirection,
            Guid? userId,
            Guid? providerId,
            string? searchValue,
            bool? isActive,
            ContactMessageStatus? contactStatus,
            DateTime? fromDate,
            DateTime? toDate);
        Task<ContactMessage?> GetByIdForAdminAsync(Guid id);
    }
}