using CocoQR.Domain.Constants.Enum;

namespace CocoQR.Infrastructure.BackgroundServices.BackgroundQueueWorker.Jobs
{
    public sealed class SendEmailJob : BackgroundJob
    {
        public SendEmailJob()
        {
            JobType = BackgroundJobTypes.SendEmail;
        }

        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public EmailDirection Direction { get; set; } = EmailDirection.OUTGOING;
        public string? TemplateKey { get; set; }
        public CocoQR.Domain.Constants.Enum.SmtpSettingType? SmtpType { get; set; }
        public Guid? EmailLogId { get; set; }
        public bool IsPrepared { get; set; } = false;
        public SmtpPayload? Smtp { get; set; }
    }

    public class SmtpPayload
    {
        public string Host { get; set; } = "";
        public int Port { get; set; }
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public bool UseSsl { get; set; }
        public string FromEmail { get; set; } = "";
        public string FromName { get; set; } = "";
    }
}
