using CocoQR.Application.DTOs.Contacts.Requests;
using CocoQR.Application.DTOs.Contacts.Responses;
using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Domain.Constants.Enum;

namespace CocoQR.Application.Contracts.IServices
{
    public interface IContactService
    {
        Task<PagingVM<GetContactMessageRes>> GetAllAsync(
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
        Task<GetContactMessageRes> GetByIdAsync(Guid id);
        Task ContactToSystemAsync(ContactRequest request);
        Task ContactFromSystemAsync(AdminContactRequest request);
        Task IgnoreContactMessageAsync(Guid contactMessageId);
    }
}