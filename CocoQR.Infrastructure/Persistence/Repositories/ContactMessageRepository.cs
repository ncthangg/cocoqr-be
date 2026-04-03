using CocoQR.Application.Contracts.IRepositories;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Domain.Entities;
using CocoQR.Infrastructure.Persistence.Repositories.Base;

namespace CocoQR.Infrastructure.Persistence.Repositories
{
    public class ContactMessageRepository : BaseRepository<ContactMessage>, IContactMessageRepository
    {
        public ContactMessageRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "ContactMessages")
        {
        }

        public async Task<IEnumerable<ContactMessage>> GetAllForAdminAsync()
        {
            const string sql = @"
                SELECT
                    Id,
                    FullName,
                    Email,
                    Status,
                    CreatedAt,
                    RepliedAt
                FROM ContactMessages
                ORDER BY CreatedAt DESC
            ";

            return await QueryAsync<ContactMessage>(sql);
        }

        public async Task<ContactMessage?> GetByIdForAdminAsync(Guid id)
        {
            const string sql = @"
                SELECT TOP 1
                    Id,
                    FullName,
                    Email,
                    Content,
                    Status,
                    CreatedAt,
                    RepliedAt
                FROM ContactMessages
                WHERE Id = @Id
            ";

            return await QueryFirstOrDefaultAsync<ContactMessage>(sql, new { Id = id });
        }
    }
}