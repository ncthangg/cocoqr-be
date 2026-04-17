using CocoQR.Application.DTOs.Settings;
using CocoQR.Infrastructure.BackgroundServices.BackgroundQueueWorker.Jobs;
using CocoQR.Infrastructure.SubService;

namespace CocoQR.Infrastructure.BackgroundServices.BackgroundQueueWorker.Handlers
{
    public class CleanupHandler
    {
        private readonly FileStorageService _fileStorageService;

        public CleanupHandler(FileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        public async Task HandleAsync(CleanupJob job, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(job.FilePath))
                return;

            var retryRequest = await _fileStorageService.ProcessCleanupRequestAsync(
                new FileCleanupRequest
                {
                    FilePath = job.FilePath,
                    DeleteCloud = job.DeleteCloud,
                    DeleteLocal = job.DeleteLocal,
                    Attempt = job.Attempt
                },
                enqueueOnFailure: false,
                cancellationToken: cancellationToken);

            if (retryRequest != null)
            {
                throw new InvalidOperationException($"Cleanup incomplete for {retryRequest.FilePath}");
            }
        }
    }
}
