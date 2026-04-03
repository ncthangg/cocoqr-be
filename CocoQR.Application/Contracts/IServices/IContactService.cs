using CocoQR.Application.DTOs.Contacts.Requests;
using CocoQR.Application.DTOs.Contacts.Responses;

namespace CocoQR.Application.Contracts.IServices
{
    public interface IContactService
    {
        Task<IEnumerable<GetContactMessageRes>> GetAllAsync();
        Task<GetContactMessageRes> GetByIdAsync(Guid id);
        Task ContactToSystemAsync(ContactRequest request);
        Task ContactFromSystemAsync(AdminContactRequest request);
    }
}