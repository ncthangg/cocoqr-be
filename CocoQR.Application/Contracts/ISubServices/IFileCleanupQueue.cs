using CocoQR.Application.DTOs.Settings;

namespace CocoQR.Application.Contracts.ISubServices
{
    public interface IFileCleanupQueue
    {
        ValueTask EnqueueAsync(FileCleanupRequest request, CancellationToken cancellationToken = default);
        IAsyncEnumerable<FileCleanupRequest> DequeueAllAsync(CancellationToken cancellationToken);
    }
}
