using CocoQR.Infrastructure.BackgroundServices.BackgroundQueueWorker.Jobs;

namespace CocoQR.Infrastructure.BackgroundServices.BackgroundQueueWorker
{
    public sealed class UploadLogJob : BackgroundJob
    {
        public UploadLogJob()
        {
            JobType = BackgroundJobTypes.UploadLog;
        }

        public string FilePath { get; set; } = string.Empty;
    }
}
