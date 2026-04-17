namespace CocoQR.Infrastructure.BackgroundServices.BackgroundQueueWorker
{
    public static class BackgroundJobTypes
    {
        public const string UploadLog = "UploadLogJob";
        public const string Cleanup = "CleanupJob";
        public const string UploadAsset = "UploadAssetJob";
        public const string SendEmail = "SendEmailJob";
    }
}
