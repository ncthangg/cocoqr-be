using Dapper;
using CocoQR.Application.Contracts.IRepositories.Base;
using CocoQR.Application.Contracts.IUnitOfWork;
using System.Data;

namespace CocoQR.Infrastructure.Persistence.Repositories.Base
{
    public abstract class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly string _tableName;

        protected BaseRepository(IUnitOfWork unitOfWork, string tableName)
        {
            _unitOfWork = unitOfWork;
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

            return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<TEntity>(
                sql,
                new { Id = id },
                _unitOfWork.Transaction
            );
        }

        /// <summary>
        /// Generic SELECT ALL
        /// </summary>
        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            string sql = $"SELECT * FROM {_tableName}";

            return await _unitOfWork.Connection.QueryAsync<TEntity>(
                sql,
                _unitOfWork.Transaction
            );

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
                var value = prop.GetValue(entity);

                if (value != null && value.GetType().IsEnum)
                {
                    value = value.ToString();
                }

                parameters.Add($"@{prop.Name}", value);
            }

            await _unitOfWork.Connection.ExecuteAsync(sql, parameters, _unitOfWork.Transaction);
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
                var value = prop.GetValue(entity);

                if (value != null && value.GetType().IsEnum)
                {
                    value = value.ToString();
                }

                parameters.Add($"@{prop.Name}", value);
            }

            await _unitOfWork.Connection.ExecuteAsync(sql, parameters, _unitOfWork.Transaction);
        }

        /// <summary>
        /// Generic DELETE
        /// </summary>
        public virtual async Task DeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Invalid ID", nameof(id));

            string sql = $"DELETE FROM {_tableName} WHERE Id = @Id";

            await _unitOfWork.Connection.ExecuteAsync(sql, new { Id = id }, _unitOfWork.Transaction);
        }
        protected async Task<TResult> QuerySingleAsync<TResult>(
            string sql,
            object? parameters = null)
        {
            return await _unitOfWork.Connection.QuerySingleAsync<TResult>(
                sql,
                parameters,
                _unitOfWork.Transaction
            );
        }
        protected async Task<TResult> QueryFirstOrDefaultAsync<TResult>(
            string sql,
            object? parameters = null)
        {
            return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<TResult>(
                sql,
                parameters,
                _unitOfWork.Transaction
            );
        }

        protected async Task<IEnumerable<TResult>> QueryAsync<TResult>(
            string sql,
            object? parameters = null)
        {
            return await _unitOfWork.Connection.QueryAsync<TResult>(
                sql,
                parameters,
                _unitOfWork.Transaction
            );
        }

        protected async Task<(IEnumerable<T>, int)> QueryPagedAsync<T>(
            string sql,
            object? parameters = null)
        {
            using var multi = await _unitOfWork.Connection.QueryMultipleAsync(
                sql,
                parameters,
                _unitOfWork.Transaction
            );

            var items = await multi.ReadAsync<T>();
            var total = await multi.ReadSingleAsync<int>();

            return (items, total);
        }

        protected async Task<int> ExecuteAsync(string sql, object? parameters = null)
        {
            return await _unitOfWork.Connection.ExecuteAsync(
                sql,
                parameters,
                _unitOfWork.Transaction
            );
        }
    }
}
