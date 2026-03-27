using CocoQR.Application.Contracts.ISubServices;
using UUIDNext;

namespace CocoQR.Infrastructure.SubService
{
    public class SqlServerIdGenerator : IIdGenerator
    {
        public Guid NewId()
            => Uuid.NewDatabaseFriendly(Database.SqlServer);
    }
}
