using CocoQR.Application.Contracts.IQueue;
using CocoQR.Domain.Constants.Enum;
using CocoQR.Infrastructure.BackgroundServices.BackgroundQueueWorker.Jobs;
using StackExchange.Redis;

namespace CocoQR.Infrastructure.BackgroundServices.BackgroundQueueWorker
{
    public class RedisBackgroundJobProducer : IBackgroundJobProducer
    {
        private readonly IQueueService _queueService;
        private readonly IConnectionMultiplexer _redis;

        public RedisBackgroundJobProducer(IQueueService queueService, IConnectionMultiplexer redis)
        {
            _queueService = queueService;
            _redis = redis;
        }

        public Task EnqueueUploadAssetAsync(Guid providerId, string? newFilePath, string? previousFilePath, CancellationToken cancellationToken = default)
        {
            return _queueService.EnqueueAsync(BackgroundQueueNames.Main, new UploadAssetJob
            {
                ProviderId = providerId,
                NewFilePath = newFilePath,
                PreviousFilePath = previousFilePath
            });
        }

        public Task EnqueueCleanupAsync(string filePath, bool deleteCloud, bool deleteLocal, int attempt = 0, CancellationToken cancellationToken = default)
        {
            return _queueService.EnqueueAsync(BackgroundQueueNames.Main, new CleanupJob
            {
                FilePath = filePath,
                DeleteCloud = deleteCloud,
                DeleteLocal = deleteLocal,
                Attempt = attempt
            });
        }

        public Task EnqueueUploadLogAsync(string filePath, CancellationToken cancellationToken = default)
        {
            return _queueService.EnqueueAsync(BackgroundQueueNames.Main, new UploadLogJob
            {
                FilePath = filePath
            });
        }

        public Task EnqueueSendEmailAsync(string to, string subject, string body, EmailDirection direction, string? templateKey = null, SmtpSettingType? smtpType = null, Guid? emailLogId = null, CancellationToken cancellationToken = default)
        {
            if (!_redis.IsConnected)
            {
                throw new InvalidOperationException("Redis is not connected.");
            }

            return _queueService.EnqueueAsync(BackgroundQueueNames.Main, new SendEmailJob
            {
                To = to,
                Subject = subject,
                Body = body,
                Direction = direction,
                TemplateKey = templateKey,
                SmtpType = smtpType,
                EmailLogId = emailLogId,
                IsPrepared = false,
            });
        }
    }
}
