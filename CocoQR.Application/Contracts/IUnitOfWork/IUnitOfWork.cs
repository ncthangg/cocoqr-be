using CocoQR.Application.Contracts.IRepositories;
using System.Data;

namespace CocoQR.Application.Contracts.IUnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IDbConnection Connection { get; }
        IDbTransaction? Transaction { get; }

        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        IAccountRepository Accounts { get; }
        IQrRepository QRHistories { get; }
        IQRStyleRepository QRStyles { get; }
        IQRStyleLibraryRepository QRStyleLibraries { get; }
        IBankInfoRepository BankInfos { get; }
        IProviderRepository Providers { get; }

        IUserTokenRepository UserTokens { get; }
        IUserRoleRepository UserRoles { get; }

        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
