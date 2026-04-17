using CocoQR.Application.Contracts.ICache;
using CocoQR.Domain.Constants;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CocoQR.Infrastructure.Redis.Cache
{
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                if (!_redis.IsConnected)
                {
                    _logger.LogWarning("Redis not connected, bypass cache");
                    return default;
                }
                var db = _redis.GetDatabase();

                var value = await db.StringGetAsync(key);
                if (value.IsNullOrEmpty) return default;

                return JsonSerializer.Deserialize<T>(value);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogWarning(ex, "Redis unavailable, bypass cache");
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                if (!_redis.IsConnected)
                {
                    _logger.LogWarning("Redis not connected, bypass cache");
                    return;
                }
                var db = _redis.GetDatabase();

                var json = JsonSerializer.Serialize(value);
                await db.StringSetAsync(key, json, (Expiration)expiry);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogWarning(ex, "Redis unavailable, bypass cache");
            }
        }
         
        public async Task RemoveAsync(string key)
        {
            try
            {
                if (!_redis.IsConnected)
                {
                    _logger.LogWarning("Redis not connected, bypass cache");
                    return;
                }
                var db = _redis.GetDatabase();

                await db.KeyDeleteAsync(key);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogWarning(ex, "Redis unavailable, bypass cache");
            }
        }
    }
}
