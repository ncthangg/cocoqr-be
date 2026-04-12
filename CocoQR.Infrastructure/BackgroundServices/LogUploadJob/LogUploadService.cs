using CocoQR.Application.Contracts.IQueue;
using CocoQR.Application.Contracts.ICache;
using CocoQR.Application.Contracts.ISubServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using static CocoQR.Domain.Constants.FileStorage;

namespace CocoQR.Infrastructure.BackgroundServices.LogUploadJob
{
    public class LogUploadService : BackgroundService
    {
        private static readonly string[] LogLevelFolders = { Folders.Info, Folders.Warning, Folders.Error };
        private const string LogUploadQueue = "log-upload";
        private static readonly TimeSpan ScanInterval = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan EnqueuedCacheTtl = TimeSpan.FromHours(6);

        private readonly IHostEnvironment _env;
        private readonly ILogger<LogUploadService> _logger;
        private readonly IFileStorageService _fileStorageService;
        private readonly IQueueService _queueService;
        private readonly ICacheService _cacheService;

        public LogUploadService(IHostEnvironment env, ILogger<LogUploadService> logger, IFileStorageService fileStorageService, IQueueService queueService, ICacheService cacheService)
        {
            _env = env;
            _logger = logger;
            _fileStorageService = fileStorageService;
            _queueService = queueService;
            _cacheService = cacheService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_env.IsStaging() && !_env.IsProduction())
                return;

            _logger.LogInformation(
                "LogUploadService started in {Environment} at {UtcNow}. Log folder: {LogFolder}",
                _env.EnvironmentName,   
                DateTime.UtcNow,
                GetLogFolder());

            var nextScanAt = DateTime.UtcNow;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (DateTime.UtcNow >= nextScanAt)
                    {
                        _logger.LogInformation("[LogFlow:Scan] Start scanning logs at {UtcNow}", DateTime.UtcNow);
                        await ScanLogsAndEnqueue(stoppingToken);
                        _logger.LogInformation("[LogFlow:Scan] Finish scanning logs at {UtcNow}", DateTime.UtcNow);
                        nextScanAt = DateTime.UtcNow.Add(ScanInterval);
                    }

                    var job = await _queueService.DequeueAsync<LogUploadJob>(LogUploadQueue);

                    if (job != null)
                    {
                        _logger.LogInformation("[LogFlow:Dequeue] Received job for file {FilePath}", job.FilePath);
                        await ProcessUploadJobAsync(job, stoppingToken);
                    }
                    else
                    {
                        await Task.Delay(1000, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing log upload queue");
                }
            }
        }

        private async Task ProcessUploadJobAsync(LogUploadJob job, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(job.FilePath))
            {
                _logger.LogWarning("[LogFlow:Worker] Job has empty file path, skipped");
                return;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("[LogFlow:Worker] Cancellation requested, stop processing file {FilePath}", job.FilePath);
                return;
            }

            var cacheKey = GetEnqueuedCacheKey(job.FilePath);

            try
            {
                if (!File.Exists(job.FilePath))
                {
                    _logger.LogWarning("[LogFlow:Worker] File not found, remove queue marker: {FilePath}", job.FilePath);
                    await _cacheService.RemoveAsync(cacheKey);
                    return;
                }

                try
                {
                    _logger.LogInformation("[LogFlow:Upload] Start upload file {FilePath}", job.FilePath);
                    await _fileStorageService.UploadLogFileToCloudAsync(job.FilePath);
                    _logger.LogInformation("[LogFlow:Upload] Upload success for file {FilePath}", job.FilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload log file: {File}", job.FilePath);
                    _logger.LogWarning("[LogFlow:Retry] Re-enqueue file {FilePath}", job.FilePath);

                    await _queueService.EnqueueAsync(LogUploadQueue, job);

                    return;
                }

                _logger.LogInformation("[LogFlow:Delete] Deleting local file {FilePath}", job.FilePath);
                File.Delete(job.FilePath);
                _logger.LogInformation("[LogFlow:Delete] Deleted local file {FilePath}", job.FilePath);

                await _cacheService.RemoveAsync(cacheKey);
                _logger.LogDebug("[LogFlow:Marker] Removed queued marker {CacheKey}", cacheKey);

                _logger.LogInformation("Uploaded and deleted log file: {File}", job.FilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload log file: {File}", job.FilePath);
            }
        }

        private async Task ScanLogsAndEnqueue(CancellationToken cancellationToken)
        {
            var logFolder = GetLogFolder();

            if (!Directory.Exists(logFolder))
            {
                _logger.LogDebug("Log folder not found: {LogFolder}", logFolder);
                return;
            }

            var thresholdDate = DateTime.UtcNow.Date;
            var totalFound = 0;
            var totalEnqueued = 0;
            var totalSkippedQueued = 0;

            foreach (var levelFolder in LogLevelFolders)
            {
                var folderPath = Path.Combine(logFolder, levelFolder);

                if (!Directory.Exists(folderPath))
                    continue;

                var files = Directory.GetFiles(folderPath, "*.txt", SearchOption.TopDirectoryOnly)
                    .Where(file => File.GetLastWriteTimeUtc(file) < thresholdDate);

                foreach (var file in files)
                {
                    totalFound++;

                    if (cancellationToken.IsCancellationRequested)
                        return;

                    var cacheKey = GetEnqueuedCacheKey(file);
                    var alreadyQueued = await _cacheService.GetAsync<bool>(cacheKey);
                    if (alreadyQueued)
                    {
                        totalSkippedQueued++;
                        _logger.LogDebug("[LogFlow:Enqueue] Skip already queued file {FilePath}", file);
                        continue;
                    }

                    await _queueService.EnqueueAsync(
                        LogUploadQueue,
                        new LogUploadJob
                        {
                            FilePath = file
                        });

                    await _cacheService.SetAsync(cacheKey, true, EnqueuedCacheTtl);
                    totalEnqueued++;
                    _logger.LogInformation("[LogFlow:Enqueue] Enqueued file {FilePath} with marker {CacheKey}", file, cacheKey);
                }
            }

            _logger.LogInformation(
                "[LogFlow:ScanSummary] Found={Found}, Enqueued={Enqueued}, SkippedQueued={Skipped}, ThresholdDate={ThresholdDate}",
                totalFound,
                totalEnqueued,
                totalSkippedQueued,
                thresholdDate);
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

        private static string GetEnqueuedCacheKey(string filePath)
        {
            var normalized = filePath.Trim().ToLowerInvariant();
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
            var hash = Convert.ToHexString(bytes);
            return $"log-upload:queued:{hash}";
        }
    }
}
