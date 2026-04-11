using CocoQR.Application.Contracts.IRepositories;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Domain.Entities;
using CocoQR.Infrastructure.Persistence.Repositories.Base;

namespace CocoQR.Infrastructure.Persistence.Repositories
{
    public class EmailTemplateRepository : BaseRepository<EmailTemplate>, IEmailTemplateRepository
    {
        public EmailTemplateRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "EmailTemplates")
        {
        }

        public async Task<EmailTemplate?> GetByKeyAsync(string templateKey, bool onlyActive = true)
        {
            if (string.IsNullOrWhiteSpace(templateKey))
            {
                throw new ArgumentException("Template key is required", nameof(templateKey));
            }

            const string sql = @"
                SELECT TOP 1
                    Id,
                    TemplateKey,
                    Subject,
                    Body,
                    IsActive,
                    UpdatedAt
                FROM EmailTemplates
                WHERE TemplateKey = @TemplateKey
                  AND (@OnlyActive = 0 OR IsActive = 1)
            ";

            return await QueryFirstOrDefaultAsync<EmailTemplate>(sql, new
            {
                TemplateKey = templateKey.Trim(),
                OnlyActive = onlyActive
            });
        }
    }
}
