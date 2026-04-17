using CocoQR.Application.Contracts.ISubServices;

namespace CocoQR.Infrastructure.BackgroundServices.BackgroundQueueWorker.Handlers
{
    public class UploadLogHandler
    {
        private readonly IFileStorageService _fileStorageService;

        public UploadLogHandler(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        public async Task HandleAsync(UploadLogJob job, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(job.FilePath) || !File.Exists(job.FilePath))
                return;

            cancellationToken.ThrowIfCancellationRequested();
            await _fileStorageService.UploadLogFileToCloudAsync(job.FilePath);

            if (File.Exists(job.FilePath))
            {
                File.Delete(job.FilePath);
            }
        }
    }
}
