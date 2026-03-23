using MyWallet.Application.Contracts.IRepositories;
using MyWallet.Application.Contracts.IUnitOfWork;
using MyWallet.Domain.Entities;
using MyWallet.Infrastructure.Persistence.Repositories.Base;

namespace MyWallet.Infrastructure.Persistence.Repositories
{
    public class RoleRepository : BaseRepository<Role>, IRoleRepository
    {
        public RoleRepository(IUnitOfWork _unitOfWork)
                       : base(_unitOfWork, "Roles")
        {
        }
        public async Task<Role?> GetByNameAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Role name cannot be empty", nameof(roleName));

            const string sql = @"
        SELECT 
            Id,
            Name,
            NameUpperCase,
            CreatedAt,
            Status
        FROM Roles
        WHERE ( Name = @Name AND NameUpperCase = @NameUpperCase )
        ";

            return await QueryFirstOrDefaultAsync<Role>(sql,
                new
                {
                    Name = roleName.Trim().ToLower(),
                    NameUpperCase = roleName.Trim().ToUpper()
                }
            );
        }
    }
}
