using System.Data;

namespace CocoQR.Application.Contracts.IDbContext
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
        Task<IDbConnection> CreateConnectionAsync();
    }
}
