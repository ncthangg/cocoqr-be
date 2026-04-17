using CocoQR.Application.Contracts.ICache;
using CocoQR.Application.Contracts.IQueue;
using CocoQR.Domain.Constants.Enum;
using CocoQR.Infrastructure.BackgroundServices.BackgroundQueueWorker.Handlers;
using CocoQR.Infrastructure.BackgroundServices.BackgroundQueueWorker.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CocoQR.Infrastructure.BackgroundServices.BackgroundQueueWorker
{
    public class BackgroundQueueWorker : BackgroundService
    {
        private readonly IHostEnvironment _env;
        private readonly IQueueService _queueService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ICacheService _cacheService;
        private readonly ILogger<BackgroundQueueWorker> _logger;

        public BackgroundQueueWorker(
            IHostEnvironment env,
            IQueueService queueService,
            IServiceScopeFactory scopeFactory,
            ICacheService cacheService,
            ILogger<BackgroundQueueWorker> logger)
        {
            _env = env;
            _queueService = queueService;
            _scopeFactory = scopeFactory;
            _cacheService = cacheService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BackgroundQueueWorker started in {Environment}", _env.EnvironmentName);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var payload = await _queueService.DequeueAsync<JsonElement>(BackgroundQueueNames.Main);

                    if (payload.ValueKind == JsonValueKind.Undefined || payload.ValueKind == JsonValueKind.Null)
                    {
                        await Task.Delay(1000, stoppingToken);
                        continue;
                    }

                    var jobType = GetJobType(payload);
                    if (string.IsNullOrWhiteSpace(jobType))
                    {
                        _logger.LogWarning("Background job without JobType was ignored.");
                        continue;
                    }

                    await ProcessByTypeAsync(jobType, payload, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "BackgroundQueueWorker loop error");
                }
            }
        }

        private async Task ProcessByTypeAsync(string jobType, JsonElement payload, CancellationToken cancellationToken)
        {
            switch (jobType)
            {
                case BackgroundJobTypes.UploadLog:
                    await ProcessAsync<UploadLogJob, UploadLogHandler>(payload, (handler, job, ct) => handler.HandleAsync(job, ct), cancellationToken);
                    break;

                case BackgroundJobTypes.Cleanup:
                    await ProcessAsync<CleanupJob, CleanupHandler>(payload, (handler, job, ct) => handler.HandleAsync(job, ct), cancellationToken);
                    break;

                case BackgroundJobTypes.UploadAsset:
                    await ProcessAsync<UploadAssetJob, UploadAssetHandler>(payload, (handler, job, ct) => handler.HandleAsync(job, ct), cancellationToken);
                    break;

                case BackgroundJobTypes.SendEmail:
                    await ProcessAsync<SendEmailJob, EmailHandler>(payload, (handler, job, ct) => handler.HandleAsync(job, ct), cancellationToken);
                    break;

                default:
                    _logger.LogWarning("Unsupported background job type: {JobType}", jobType);
                    break;
            }
        }

        private async Task ProcessAsync<TJob, THandler>(
            JsonElement payload,
            Func<THandler, TJob, CancellationToken, Task> execute,
            CancellationToken cancellationToken)
            where TJob : BackgroundJob
            where THandler : class
        {
            var job = payload.Deserialize<TJob>();
            if (job == null)
                return;

            var idempotencyKey = $"{GetDoneKeyPrefix(job.JobType)}:{job.JobId}";
            var shouldCheckIdempotency = ShouldCheckIdempotency(job);

            if (shouldCheckIdempotency)
            {
                var done = await _cacheService.GetAsync<bool>(idempotencyKey);
                if (done)
                {
                    _logger.LogDebug("Skipped already processed job {JobId} ({JobType})", job.JobId, job.JobType);
                    return;
                }
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<THandler>();

                _logger.LogInformation("Processing background job {JobId} ({JobType}) attempt {Attempt}", job.JobId, job.JobType, job.Attempt + 1);

                await execute(handler, job, cancellationToken);

                if (shouldCheckIdempotency)
                {
                    await _cacheService.SetAsync(idempotencyKey, true, TimeSpan.FromDays(1));
                }
                _logger.LogInformation("Background job {JobId} ({JobType}) completed", job.JobId, job.JobType);
            }
            catch (Exception ex)
            {
                if (ex is NonRetryableJobException)
                {
                    await TryMarkEmailLogFailedAsync(job, ex.Message);
                    _logger.LogWarning(
                        "Background job {JobId} ({JobType}) marked as non-retryable: {Message}",
                        job.JobId,
                        job.JobType,
                        ex.Message);
                    return;
                }

                _logger.LogError(ex, "Background job {JobId} ({JobType}) failed at attempt {Attempt}", job.JobId, job.JobType, job.Attempt + 1);

                if (job.Attempt + 1 >= job.MaxRetry)
                {
                    await TryMarkEmailLogFailedAsync(job, ex.GetBaseException().Message);
                    _logger.LogError("Background job {JobId} exceeded max retry {MaxRetry}", job.JobId, job.MaxRetry);
                    return;
                }

                job.Attempt += 1;
                var delaySeconds = Math.Min(60, (int)Math.Pow(2, Math.Max(1, job.Attempt)));
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
                await _queueService.EnqueueAsync(BackgroundQueueNames.Main, job);
            }
        }

        private static string? GetJobType(JsonElement payload)
        {
            if (!payload.TryGetProperty(nameof(BackgroundJob.JobType), out var jobTypeElement))
                return null;

            return jobTypeElement.GetString();
        }

        private static string GetDoneKeyPrefix(string? jobType)
        {
            return jobType switch
            {
                BackgroundJobTypes.UploadAsset => "bg:done:asset",
                BackgroundJobTypes.UploadLog => "bg:done:log",
                BackgroundJobTypes.SendEmail => "bg:done:sendmail",
                BackgroundJobTypes.Cleanup => "bg:done:cleanup",
                _ => "bg:done:other"
            };
        }

        private static bool ShouldCheckIdempotency(BackgroundJob job)
        {
            return job.JobType != BackgroundJobTypes.SendEmail;
        }

        private async Task TryMarkEmailLogFailedAsync<TJob>(TJob job, string errorMessage)
            where TJob : BackgroundJob
        {
            if (job is not SendEmailJob emailJob || !emailJob.EmailLogId.HasValue)
                return;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<CocoQR.Application.Contracts.IUnitOfWork.IUnitOfWork>();
                var emailLog = await unitOfWork.EmailLogs.GetByIdAsync(emailJob.EmailLogId.Value);
                if (emailLog == null)
                    return;

                emailLog.Status = EmailLogStatus.FAIL;
                emailLog.ErrorMessage = errorMessage.Length <= 2000 ? errorMessage : errorMessage[..2000];
                await unitOfWork.EmailLogs.UpdateAsync(emailLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mark EmailLog as FAIL for job {JobId}", job.JobId);
            }
        }
    }
}
