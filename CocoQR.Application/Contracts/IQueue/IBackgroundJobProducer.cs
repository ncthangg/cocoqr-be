namespace CocoQR.Application.Contracts.IQueue
{
    public interface IBackgroundJobProducer
    {
        Task EnqueueUploadAssetAsync(Guid providerId, string? newFilePath, string? previousFilePath, CancellationToken cancellationToken = default);
        Task EnqueueCleanupAsync(string filePath, bool deleteCloud, bool deleteLocal, int attempt = 0, CancellationToken cancellationToken = default);
        Task EnqueueUploadLogAsync(string filePath, CancellationToken cancellationToken = default);
        Task EnqueueSendEmailAsync(string to, string subject, string body, CocoQR.Domain.Constants.Enum.EmailDirection direction, string? templateKey = null, CocoQR.Domain.Constants.Enum.SmtpSettingType? smtpType = null, Guid? emailLogId = null, CancellationToken cancellationToken = default);
    }
}
