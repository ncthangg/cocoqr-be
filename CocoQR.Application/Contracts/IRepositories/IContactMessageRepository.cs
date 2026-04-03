using CocoQR.Application.Contracts.IRepositories.Base;
using CocoQR.Domain.Entities;

namespace CocoQR.Application.Contracts.IRepositories
{
    public interface IContactMessageRepository : IRepository<ContactMessage>
    {
        Task<IEnumerable<ContactMessage>> GetAllForAdminAsync();
        Task<ContactMessage?> GetByIdForAdminAsync(Guid id);
    }
}