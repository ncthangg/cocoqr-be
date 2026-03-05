using Dapper;
using MyWallet.Domain.Interface.IDbContext;
using MyWallet.Domain.Interface.IRepositories.Base;
using System.Data;

namespace MyWallet.Infrastructure.Persistence.Repositories.Base
{
    public abstract class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly IDbConnectionFactory _connectionFactory;
        protected readonly string _tableName;

        protected BaseRepository(IDbConnectionFactory connectionFactory, string tableName)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(_connectionFactory));
            _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        }

        /// <summary>
        /// Generic SELECT by ID
        /// </summary>
        public virtual async Task<TEntity> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Invalid ID", nameof(id));

            string sql = $"SELECT * FROM {_tableName} WHERE Id = @Id";

            // ✅ Use factory to create connection
            using (var connection = _connectionFactory.CreateConnection())
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                return await connection.QueryFirstOrDefaultAsync<TEntity>(sql,new { Id = id });
            }
        }

        /// <summary>
        /// Generic SELECT ALL
        /// </summary>
        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            string sql = $"SELECT * FROM {_tableName}";

            using (var connection = _connectionFactory.CreateConnection())
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                return await connection.QueryAsync<TEntity>(sql);
            }
        }

        /// <summary>
        /// Generic INSERT
        /// </summary>
        public virtual async Task AddAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var properties = typeof(TEntity).GetProperties()
                                            .Where(p =>
                                                p.CanRead &&
                                                (
                                                    p.PropertyType.IsValueType ||
                                                    p.PropertyType == typeof(string)
                                                )
                                            )
                                            .ToList();

            var columnNames = string.Join(", ", properties.Select(p => p.Name));
            var parameterNames = string.Join(", ", properties.Select(p => $"@{p.Name}"));

            string sql = $"INSERT INTO {_tableName} ({columnNames}) VALUES ({parameterNames})";

            var parameters = new DynamicParameters();
            foreach (var prop in properties)
            {
                parameters.Add($"@{prop.Name}", prop.GetValue(entity));
            }

            using (var connection = _connectionFactory.CreateConnection())
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                await connection.ExecuteAsync(sql, parameters);
            }
        }

        /// <summary>
        /// Generic UPDATE
        /// </summary>
        public virtual async Task UpdateAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var properties = typeof(TEntity).GetProperties()
            .Where(p => p.Name != "Id"
                   && p.CanRead
                   && (p.PropertyType.IsValueType || p.PropertyType == typeof(string)))
            .ToList();


            var setClause = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));

            string sql = $"UPDATE {_tableName} SET {setClause} WHERE Id = @Id";

            var parameters = new DynamicParameters();
            var idProperty = typeof(TEntity).GetProperty("Id");
            parameters.Add("@Id", idProperty?.GetValue(entity));

            foreach (var prop in properties)
            {
                parameters.Add($"@{prop.Name}", prop.GetValue(entity));
            }

            using (var connection = _connectionFactory.CreateConnection())
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                await connection.ExecuteAsync(sql, parameters);
            }
        }

        /// <summary>
        /// Generic DELETE
        /// </summary>
        public virtual async Task DeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Invalid ID", nameof(id));

            string sql = $"DELETE FROM {_tableName} WHERE Id = @Id";

            using (var connection = _connectionFactory.CreateConnection())
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                await connection.ExecuteAsync(sql, new { Id = id });
            }
        }
        /// <summary>
        /// Execute custom query with parameters
        /// ⚡ SAFE - Parametrized automatically
        /// </summary>
        protected async Task<TResult> QuerySingleAsync<TResult>(
            string sql,
            object? parameters = null)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                return await connection.QueryFirstOrDefaultAsync<TResult>(sql, parameters);
            }
        }

        /// <summary>
        /// Execute custom query with multiple results
        /// ⚡ SAFE - Parametrized automatically
        /// </summary>
        protected async Task<IEnumerable<TResult>> QueryAsync<TResult>(
            string sql,
            object? parameters = null)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                return await connection.QueryAsync<TResult>(sql, parameters);
            }
        }

        protected async Task<(IEnumerable<T>, int)> QueryPagedAsync<T>(
            string sql,
            object? parameters = null)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                using var multi = await connection.QueryMultipleAsync(sql, parameters);

                var items = await multi.ReadAsync<T>();
                var total = await multi.ReadSingleAsync<int>();

                return (items, total);
            }
        }

        /// <summary>
        /// Execute command (INSERT, UPDATE, DELETE)
        /// ⚡ SAFE - Parametrized automatically
        /// </summary>
        protected async Task<int> ExecuteAsync(string sql, object? parameters = null)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                return await connection.ExecuteAsync(sql, parameters);
            }
        }
    }
}
