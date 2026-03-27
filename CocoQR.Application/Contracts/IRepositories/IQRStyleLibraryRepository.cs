using CocoQR.Domain.Constants.Enum;
using CocoQR.Domain.Entities;

namespace CocoQR.Application.Contracts.IRepositories
{
    public interface IQRStyleLibraryRepository
    {
        Task<IEnumerable<QRStyleLibrary>> GetAllAsync(Guid? userId, QRStyleType? type, bool? isActive, bool isAdmin);

        Task<QRStyleLibrary?> GetByIdAsync(Guid id);
        Task AddAsync(QRStyleLibrary entity);
        Task UpdateAsync(QRStyleLibrary entity);
        Task DeleteAsync(Guid id);
    }
}
