using MyWallet.Domain.Interface.IRepositories;
using MyWallet.Domain.Interface.IUnitOfWork;
using MyWallet.Infrastructure.Persistence.Repositories;
using System.Data;
using IDbConnectionFactory = MyWallet.Domain.Interface.IDbContext.IDbConnectionFactory;

namespace MyWallet.Infrastructure.Persistence.UnitOfWork
{
    public class UnitOfWork(IDbConnectionFactory connectionFactory) : IUnitOfWork
    {
        private readonly IDbConnectionFactory _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        private IDbConnection? _connection;
        private IDbTransaction? _transaction;

        private IUserRepository? _userRepository;
        private IAccountRepository? _accountRepository;
        private IRoleRepository? _roleRepository;
        private IUserTokenRepository? _userTokenRepository;
        private IUserRoleRepository? _userRoleRepository;
        private IQRHistoryRepository? _qrHistoryRepository;
        private IBankInfoRepository? _bankInfoRepository;

        public IUserRepository Users
            => _userRepository ??= new UserRepository(_connectionFactory);

        public IAccountRepository Accounts
            => _accountRepository ??= new AccountRepository(_connectionFactory);
        public IRoleRepository Roles
            => _roleRepository ??= new RoleRepository(_connectionFactory);

        public IUserTokenRepository UserTokens
            => _userTokenRepository ??= new UserTokenRepository(_connectionFactory);
        public IUserRoleRepository UserRoles
            => _userRoleRepository ??= new UserRoleRepository(_connectionFactory);

        public IQRHistoryRepository QRHistories
            => _qrHistoryRepository ??= new QRHistoryRepository(_connectionFactory);

        public IBankInfoRepository BankInfos
            => _bankInfoRepository ??= new BankInfoRepository(_connectionFactory);

        public async Task BeginTransactionAsync()
        {
            if (_connection == null)
            {
                _connection = await _connectionFactory.CreateConnectionAsync();
            }

            if (_transaction == null)
            {
                _transaction = _connection.BeginTransaction();
            }
        }

        public Task CommitAsync()
        {
            try
            {
                _transaction?.Commit();
                return Task.CompletedTask;
            }
            finally
            {
                Dispose();
            }
        }

        public Task RollbackAsync()
        {
            try
            {
                _transaction?.Rollback();
                return Task.CompletedTask;
            }
            finally
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _connection?.Dispose();
            _transaction = null;
            _connection = null;
        }
    }
}
