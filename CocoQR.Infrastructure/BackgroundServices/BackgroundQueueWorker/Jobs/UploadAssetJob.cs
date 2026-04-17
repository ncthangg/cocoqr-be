namespace CocoQR.Infrastructure.BackgroundServices.BackgroundQueueWorker.Jobs
{
    public sealed class UploadAssetJob : BackgroundJob
    {
        public UploadAssetJob()
        {
            JobType = BackgroundJobTypes.UploadAsset;
        }

        public Guid ProviderId { get; set; }
        public string? NewFilePath { get; set; }
        public string? PreviousFilePath { get; set; }
    }
}
