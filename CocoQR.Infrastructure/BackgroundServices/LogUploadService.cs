using CocoQR.Application.Contracts.ISubServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static CocoQR.Domain.Constants.FileStorage;

namespace CocoQR.Infrastructure.BackgroundServices
{
    public class LogUploadService : BackgroundService
    {
        private static readonly string[] LogLevelFolders = { Folders.Info, Folders.Warning, Folders.Error };

        private readonly IHostEnvironment _env;
        private readonly ILogger<LogUploadService> _logger;
        private readonly IFileStorageService _fileStorageService;

        public LogUploadService(IHostEnvironment env, ILogger<LogUploadService> logger, IFileStorageService fileStorageService)
        {
            _env = env;
            _logger = logger;
            _fileStorageService = fileStorageService;
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

            while (!stoppingToken.IsCancellationRequested)
            {

                await UploadOldLogs(stoppingToken);

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task UploadOldLogs(CancellationToken cancellationToken)
        {
            var logFolder = GetLogFolder();

            if (!Directory.Exists(logFolder))
            {
                _logger.LogDebug("Log folder not found: {LogFolder}", logFolder);
                return;
            }

            var thresholdDate = DateTime.UtcNow.Date;

            foreach (var levelFolder in LogLevelFolders)
            {
                var folderPath = Path.Combine(logFolder, levelFolder);
                if (!Directory.Exists(folderPath))
                {
                    continue;
                }

                var files = Directory.GetFiles(folderPath, "*.txt", SearchOption.TopDirectoryOnly)
                    .Where(file => File.GetLastWriteTimeUtc(file) < thresholdDate)
                    .Where(file => !File.Exists(GetUploadedMarker(file)));

                foreach (var file in files)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    try
                    {
                        await _fileStorageService.UploadLogFileToCloudAsync(file);
                        await File.WriteAllTextAsync(GetUploadedMarker(file), DateTime.UtcNow.ToString("O"), cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to upload log file: {File}", file);
                    }
                }
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

        private static string GetUploadedMarker(string filePath)
        {
            return $"{filePath}.uploaded";
        }
    }
}
