namespace CocoQR.Application.Contracts.IQueue
{
    public interface IBackgroundJobProducer
    {
        Task EnqueueUploadAssetAsync(Guid providerId, string? newFilePath, string? previousFilePath, CancellationToken cancellationToken = default);
        Task EnqueueCleanupAsync(string filePath, bool deleteCloud, bool deleteLocal, int attempt = 0, CancellationToken cancellationToken = default);
        Task EnqueueUploadLogAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
