using CocoQR.Application.Contracts.IRepositories;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Domain.Entities;
using CocoQR.Infrastructure.Persistence.Repositories.Base;

namespace CocoQR.Infrastructure.Persistence.Repositories
{
    public class CocoMailCallbackEventRepository
        : BaseRepository<CocoMailCallbackEvent>, ICocoMailCallbackEventRepository
    {
        public CocoMailCallbackEventRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "CocoMailCallbackEvents")
        {
        }

        public async Task<bool> ExistsByEventIdAsync(string eventId)
        {
            const string sql = """
                SELECT COUNT(1)
                FROM CocoMailCallbackEvents
                WHERE EventId = @EventId;
                """;

            var count = await QuerySingleAsync<int>(sql, new { EventId = eventId });
            return count > 0;
        }

        public async Task MarkProcessedAsync(string eventId, DateTime processedAt)
        {
            const string sql = """
                UPDATE CocoMailCallbackEvents
                SET ProcessedAt = @ProcessedAt
                WHERE EventId = @EventId;
                """;

            await ExecuteAsync(sql, new
            {
                EventId = eventId,
                ProcessedAt = processedAt
            });
        }
    }
}
