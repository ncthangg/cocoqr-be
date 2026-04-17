using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Infrastructure.BackgroundServices.BackgroundQueueWorker.Jobs;

namespace CocoQR.Infrastructure.BackgroundServices.BackgroundQueueWorker.Handlers
{
    public class UploadAssetHandler
    {
        private readonly IFileStorageService _fileStorageService;

        public UploadAssetHandler(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        public async Task HandleAsync(UploadAssetJob job, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!string.IsNullOrWhiteSpace(job.PreviousFilePath))
            {
                await _fileStorageService.DeleteFileAsync(job.PreviousFilePath);
            }
        }
    }
}
