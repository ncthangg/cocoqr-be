namespace CocoQR.Infrastructure.BackgroundServices.BackgroundQueueWorker.Jobs
{
    public sealed class CleanupJob : BackgroundJob
    {
        public CleanupJob()
        {
            JobType = BackgroundJobTypes.Cleanup;
        }

        public string FilePath { get; set; } = string.Empty;
        public bool DeleteCloud { get; set; }
        public bool DeleteLocal { get; set; } = true;
    }
}
