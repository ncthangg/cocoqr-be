using CocoQR.Application.Contracts.IQueue;
using CocoQR.Infrastructure.BackgroundServices.BackgroundQueueWorker.Jobs;

namespace CocoQR.Infrastructure.BackgroundServices.BackgroundQueueWorker
{
    public class RedisBackgroundJobProducer : IBackgroundJobProducer
    {
        private readonly IQueueService _queueService;

        public RedisBackgroundJobProducer(IQueueService queueService)
        {
            _queueService = queueService;
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
    }
}
