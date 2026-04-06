using CocoQR.Application.Contracts.IQueue;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CocoQR.Infrastructure.Redis.Queue
{
    public class RedisQueueService : IQueueService
    {
        private readonly IDatabase _db;

        public RedisQueueService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task EnqueueAsync<T>(string queue, T data)
        {
            var json = JsonSerializer.Serialize(data);
            await _db.ListRightPushAsync(queue, json);
        }

        public async Task<T?> DequeueAsync<T>(string queue)
        {
            var value = await _db.ListLeftPopAsync(queue);

            if (value.IsNullOrEmpty)
                return default;

            return JsonSerializer.Deserialize<T>(value);
        }
    }
}
