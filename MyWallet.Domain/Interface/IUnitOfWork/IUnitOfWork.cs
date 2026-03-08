using MyWallet.Domain.Interface.IRepositories;
using System.Data;

namespace MyWallet.Domain.Interface.IUnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IDbConnection Connection { get; }
        IDbTransaction? Transaction { get; }

        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        IAccountRepository Accounts { get; }
        IQRHistoryRepository QRHistories { get; }
        IBankInfoRepository BankInfos { get; }

        IUserTokenRepository UserTokens { get; }
        IUserRoleRepository UserRoles { get; }

        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
