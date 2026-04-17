using CocoQR.Application.Contracts.IQueue;
using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Domain.Constants.Enum;
using CocoQR.Domain.Helper;
using CocoQR.Infrastructure.BackgroundServices.BackgroundQueueWorker.Jobs;
using Microsoft.Extensions.Logging;

namespace CocoQR.Infrastructure.BackgroundServices.BackgroundQueueWorker.Handlers
{
    public class EmailHandler
    {
        private readonly ILogger<EmailHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IQueueService _queueService;

        public EmailHandler(ILogger<EmailHandler> logger, IUnitOfWork unitOfWork,
            IQueueService queueService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _queueService = queueService;
        }

        public async Task HandleAsync(SendEmailJob job, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (job.IsPrepared)
            {
                _logger.LogWarning(
                    "Skip prepared job {JobId} in Main queue (unexpected routing)",
                    job.JobId);
                return;
            }

            var smtpType = job.SmtpType
                ?? (!string.IsNullOrWhiteSpace(job.TemplateKey)
                    ? EmailTemplateSmtpResolver.Resolve(job.TemplateKey)
                    : SmtpSettingType.System);

            var smtpSetting = await _unitOfWork.SmtpSettings.GetActiveAsync(smtpType);
            if (smtpSetting == null)
            {
                throw new NonRetryableJobException($"No active SMTP setting for background email job. Type={smtpType}");
            }

            if (!job.IsPrepared)
            {
                job.Smtp = new SmtpPayload
                {
                    Host = smtpSetting.Host,
                    Port = smtpSetting.Port,
                    Username = smtpSetting.Username,
                    Password = smtpSetting.Password,
                    UseSsl = smtpSetting.EnableSSL,
                    FromEmail = smtpSetting.FromEmail,
                    FromName = smtpSetting.FromName
                };
                job.IsPrepared = true;

                await _queueService.EnqueueAsync(BackgroundQueueNames.Local, job);

                return;
            }
        }
    }
}
//await _emailService.SendWithoutLogAsync(
//    job.To,
//    job.Subject,
//    job.Body,
//    smtpSetting,
//    job.Direction,
//    job.TemplateKey);

//if (job.EmailLogId.HasValue)
//{
//    var emailLog = await _unitOfWork.EmailLogs.GetByIdAsync(job.EmailLogId.Value);
//    if (emailLog != null)
//    {
//        emailLog.Status = EmailLogStatus.SUCCESS;
//        emailLog.ErrorMessage = null;
//        await _unitOfWork.EmailLogs.UpdateAsync(emailLog);
//    }
//}