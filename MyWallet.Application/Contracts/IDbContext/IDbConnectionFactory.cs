using System.Data;

namespace MyWallet.Application.Contracts.IDbContext
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
        Task<IDbConnection> CreateConnectionAsync();
    }
}
