using Amazon.Runtime.Internal.Util;
using CocoQR.Application.Contracts.IRateLimit;
using CocoQR.Domain.Constants;
using Microsoft.Extensions.Logging;
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
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisRateLimitService> _logger; 

        public RedisRateLimitService(IConnectionMultiplexer redis, ILogger<RedisRateLimitService> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        public async Task<bool> IsAllowedAsync(string key, int limit, TimeSpan window)
        {
            try
            {
                if (!_redis.IsConnected)
                {
                    _logger.LogWarning("Redis not connected, bypass cache");
                    return true;
                }
                var db = _redis.GetDatabase();

                var count = await db.StringIncrementAsync(key);
                if (count == 1)
                    await db.KeyExpireAsync(key, window);

                return count <= limit;
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogWarning(ex, "Redis unavailable, bypass rate limit");
                return true;
            }
        }
    }
}
