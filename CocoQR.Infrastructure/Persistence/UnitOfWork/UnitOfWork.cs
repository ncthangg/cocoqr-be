using CocoQR.Application.Contracts.IRepositories;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Infrastructure.Persistence.MyDbContext;
using CocoQR.Infrastructure.Persistence.Repositories;
using System.Data;
using IDbConnectionFactory = CocoQR.Application.Contracts.IDbContext.IDbConnectionFactory;

namespace CocoQR.Infrastructure.Persistence.UnitOfWork
{
    public class UnitOfWork(IDbConnectionFactory connectionFactory, CocoQRDbContext dbContext) : IUnitOfWork
    {
        private readonly IDbConnectionFactory _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        private readonly CocoQRDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        private IDbConnection? _connection;
        private IDbTransaction? _transaction;

        private IUserRepository? _userRepository;
        private IAccountRepository? _accountRepository;
        private IRoleRepository? _roleRepository;
        private IUserTokenRepository? _userTokenRepository;
        private IUserRoleRepository? _userRoleRepository;
        private IQrRepository? _qrHistoryRepository;
        private IQRStyleRepository? _qrStyleRepository;
        private IBankInfoRepository? _bankInfoRepository;
        private IProviderRepository? _providerRepository;
        private IQRStyleLibraryRepository? _qrStyleLibraryRepository;

        public IDbConnection Connection
        {
            get
            {
                if (_connection == null)
                    _connection = _connectionFactory.CreateConnection();

                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();

                return _connection;
            }
        }
        public IDbTransaction? Transaction => _transaction;

        public IUserRepository Users
            => _userRepository ??= new UserRepository(this);
        public IAccountRepository Accounts
            => _accountRepository ??= new AccountRepository(this);
        public IRoleRepository Roles
            => _roleRepository ??= new RoleRepository(this);
        public IUserTokenRepository UserTokens
            => _userTokenRepository ??= new UserTokenRepository(this);
        public IUserRoleRepository UserRoles
            => _userRoleRepository ??= new UserRoleRepository(this);
        public IQrRepository QRHistories
            => _qrHistoryRepository ??= new QrRepository(this);
        public IQRStyleRepository QRStyles
            => _qrStyleRepository ??= new QRStyleRepository(this);
        public IBankInfoRepository BankInfos
            => _bankInfoRepository ??= new BankInfoRepository(this);
        public IProviderRepository Providers
            => _providerRepository ??= new ProviderRepository(this);
        public IQRStyleLibraryRepository QRStyleLibraries
            => _qrStyleLibraryRepository ??= new QRStyleLibraryRepository(this, _dbContext);

        // Expose DbContext for repositories that need it
        public CocoQRDbContext DbContext => _dbContext;

        public async Task BeginTransactionAsync()
        {
            if (_connection == null)
                _connection = await _connectionFactory.CreateConnectionAsync();

            if (_connection.State == ConnectionState.Closed)
                _connection.Open();

            if (_transaction == null)
                _transaction = _connection.BeginTransaction();
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
