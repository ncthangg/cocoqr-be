using CocoQR.Application.Contracts.IRateLimit;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoQR.Infrastructure.Redis.RateLimit
{
    public class RedisRateLimitService : IRateLimitService
    {
        private readonly IDatabase _db;

        public RedisRateLimitService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task<bool> IsAllowedAsync(string key, int limit, TimeSpan window)
        {
            var count = await _db.StringIncrementAsync(key);

            if (count == 1)
            {
                await _db.KeyExpireAsync(key, window);
            }

            return count <= limit;
        }
    }
}
