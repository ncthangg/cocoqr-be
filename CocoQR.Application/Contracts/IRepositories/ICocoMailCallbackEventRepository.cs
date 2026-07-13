using CocoQR.Application.Contracts.IRepositories.Base;
using CocoQR.Domain.Entities;

namespace CocoQR.Application.Contracts.IRepositories
{
    public interface ICocoMailCallbackEventRepository : IRepository<CocoMailCallbackEvent>
    {
        Task<bool> ExistsByEventIdAsync(string eventId);
        Task MarkProcessedAsync(string eventId, DateTime processedAt);
    }
}
