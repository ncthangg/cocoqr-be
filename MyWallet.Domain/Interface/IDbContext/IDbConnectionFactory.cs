using System.Data;

namespace MyWallet.Domain.Interface.IDbContext
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
        Task<IDbConnection> CreateConnectionAsync();
    }
}
