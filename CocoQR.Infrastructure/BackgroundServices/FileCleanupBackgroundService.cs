using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Application.DTOs.Settings;
using CocoQR.Infrastructure.SubService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CocoQR.Infrastructure.BackgroundServices
{
    public class FileCleanupBackgroundService : BackgroundService
    {
        private const int MaxRetryAttempts = 5;
        private readonly IHostEnvironment _env;
        private readonly IFileCleanupQueue _cleanupQueue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<FileCleanupBackgroundService> _logger;

        public FileCleanupBackgroundService(
            IHostEnvironment env,
            IFileCleanupQueue cleanupQueue,
            IServiceScopeFactory scopeFactory,
            ILogger<FileCleanupBackgroundService> logger)
        {
            _env = env;
            _cleanupQueue = cleanupQueue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FileCleanupBackgroundService started in {Environment} at {UtcNow}", _env.EnvironmentName, DateTime.UtcNow);

            await foreach (var request in _cleanupQueue.DequeueAllAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var storageService = scope.ServiceProvider.GetRequiredService<FileStorageService>();

                    var retryRequest = await storageService.ProcessCleanupRequestAsync(
                        request,
                        enqueueOnFailure: false,
                        stoppingToken);

                    if (retryRequest == null)
                    {
                        continue;
                    }

                    if (retryRequest.Attempt >= MaxRetryAttempts)
                    {
                        _logger.LogError(
                            "Cleanup failed after {Attempt} attempts for file: {FilePath}",
                            retryRequest.Attempt,
                            retryRequest.FilePath);
                        continue;
                    }

                    _ = ScheduleRetryAsync(retryRequest, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while processing cleanup queue");
                }
            }
        }

        private async Task ScheduleRetryAsync(FileCleanupRequest request, CancellationToken cancellationToken)
        {
            var delaySeconds = Math.Min(60, (int)Math.Pow(2, Math.Max(1, request.Attempt)));

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
                await _cleanupQueue.EnqueueAsync(request, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Host shutdown.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to schedule cleanup retry for file: {FilePath}", request.FilePath);
            }
        }
    }
}
