using CocoQR.Application.Contracts.IQueue;
using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Infrastructure.BackgroundServices.BackgroundQueueWorker;
using CocoQR.Infrastructure.BackgroundServices.BackgroundQueueWorker.Jobs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static CocoQR.Domain.Constants.FileStorage;

namespace CocoQR.Infrastructure.BackgroundServices.FileCleanupJob
{
    public class FileCleanupJobProducerService : BackgroundService
    {
        private static readonly TimeSpan ScanInterval = TimeSpan.FromHours(1);

        private readonly IHostEnvironment _env;
        private readonly IFileCleanupQueue _cleanupQueue;
        private readonly IQueueService _queueService;
        private readonly ILogger<FileCleanupJobProducerService> _logger;

        public FileCleanupJobProducerService(
            IHostEnvironment env,
            IFileCleanupQueue cleanupQueue,
            IQueueService queueService,
            ILogger<FileCleanupJobProducerService> logger)
        {
            _env = env;
            _cleanupQueue = cleanupQueue;
            _queueService = queueService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_env.IsStaging() && !_env.IsProduction())
                return;

            _logger.LogInformation("FileCleanupJobProducerService started in {Environment} at {UtcNow}", _env.EnvironmentName, DateTime.UtcNow);

            _ = ProduceFromScanAsync(stoppingToken);

            await foreach (var request in _cleanupQueue.DequeueAllAsync(stoppingToken))
            {
                try
                {
                    var job = new CleanupJob
                    {
                        FilePath = request.FilePath,
                        DeleteCloud = request.DeleteCloud,
                        DeleteLocal = request.DeleteLocal,
                        Attempt = request.Attempt
                    };

                    await _queueService.EnqueueAsync(BackgroundQueueNames.Main, job);

                    _logger.LogInformation("[CleanupProducer] Enqueued cleanup job for file {FilePath}", request.FilePath);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[CleanupProducer] Failed to enqueue cleanup job for file {FilePath}", request.FilePath);
                }
            }
        }

        private async Task ProduceFromScanAsync(CancellationToken stoppingToken)
        {
            var nextScanAt = DateTime.UtcNow;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (DateTime.UtcNow >= nextScanAt)
                    {
                        await ScanAndEnqueueUnnecessaryFilesAsync(stoppingToken);
                        nextScanAt = DateTime.UtcNow.Add(ScanInterval);
                    }

                    await Task.Delay(1000, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[CleanupProducer] Error while scanning unnecessary files");
                }
            }
        }

        private async Task ScanAndEnqueueUnnecessaryFilesAsync(CancellationToken cancellationToken)
        {
            var root = GetLogFolder();
            if (!Directory.Exists(root))
                return;

            var threshold = DateTime.UtcNow.Date.AddDays(-1);
            var staleMarkers = Directory.GetFiles(root, "*.uploaded", SearchOption.AllDirectories)
                .Where(x => File.GetLastWriteTimeUtc(x) < threshold);

            foreach (var marker in staleMarkers)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                await _queueService.EnqueueAsync(BackgroundQueueNames.Main, new CleanupJob
                {
                    FilePath = marker,
                    DeleteCloud = false,
                    DeleteLocal = true
                });
            }
        }

        private string GetLogFolder()
        {
            var configuredLogPath = Environment.GetEnvironmentVariable(EnvKeys.Logs);
            if (!string.IsNullOrWhiteSpace(configuredLogPath))
            {
                if (Path.IsPathRooted(configuredLogPath))
                {
                    return Path.GetFullPath(configuredLogPath);
                }

                return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configuredLogPath));
            }

            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, Folders.Logs));
        }
    }
}
