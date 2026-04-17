using CocoQR.Application.Contracts.IQueue;
using CocoQR.Application.Contracts.ICache;
using CocoQR.Infrastructure.BackgroundServices.BackgroundQueueWorker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using static CocoQR.Domain.Constants.FileStorage;

namespace CocoQR.Infrastructure.BackgroundServices.LogUploadJob
{
    public class LogUploadJobProducerService : BackgroundService
    {
        private static readonly string[] LogLevelFolders = { Folders.Info, Folders.Warning, Folders.Error };
        private static readonly TimeSpan ScanInterval = TimeSpan.FromMinutes(60);
        private static readonly TimeSpan EnqueuedCacheTtl = TimeSpan.FromHours(6);

        private readonly IHostEnvironment _env;
        private readonly ILogger<LogUploadJobProducerService> _logger;
        private readonly IQueueService _queueService;
        private readonly ICacheService _cacheService;

        public LogUploadJobProducerService(IHostEnvironment env, ILogger<LogUploadJobProducerService> logger, IQueueService queueService, ICacheService cacheService)
        {
            _env = env;
            _logger = logger;
            _queueService = queueService;
            _cacheService = cacheService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_env.IsStaging() && !_env.IsProduction())
                return;

            _logger.LogInformation(
                "LogUploadJobProducerService started in {Environment} at {UtcNow}. Log folder: {LogFolder}",
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

                    await Task.Delay(1000, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error producing log upload jobs");
                }
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
                        BackgroundQueueNames.Main,
                        new UploadLogJob
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
