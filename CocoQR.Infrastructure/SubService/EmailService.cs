using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Domain.Constants;
using CocoQR.Domain.Constants.Enum;
using CocoQR.Domain.Entities;
using CocoQR.Domain.Exceptions;
using CocoQR.Infrastructure.Persistence.MyDbContext;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace CocoQR.Infrastructure.SubService
{
    public class EmailService : IEmailService
    {
        private const int SmtpTimeoutMs = 15000;

        private readonly CocoQRDbContext _dbContext;
        private readonly ILogger<EmailService> _logger;

        public EmailService(CocoQRDbContext dbContext, ILogger<EmailService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task SendAsync(
            string to,
            string subject,
            string body,
            SmtpSetting smtpSetting,
            EmailDirection direction = EmailDirection.OUTGOING,
            string? templateKey = null)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException(ValidationMessages.RequiredEmail, nameof(to));

            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException(ValidationMessages.RequiredSubject, nameof(subject));

            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentException(ValidationMessages.RequiredBody, nameof(body));

            ArgumentNullException.ThrowIfNull(smtpSetting);

            var emailLog = new EmailLog
            {
                Id = Guid.NewGuid(),
                ToEmail = to,
                Subject = subject,
                Body = body,
                Status = EmailLogStatus.FAIL,
                SmtpType = smtpSetting.Type,
                EmailDirection = direction,
                TemplateKey = templateKey,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                if (!smtpSetting.IsActive)
                {
                    throw new DomainException(
                        ErrorCode.BusinessRuleViolation,
                        ErrorMessages.SmtpSettingInactive,
                        data: new
                        {
                            smtpSetting.Type
                        });
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(smtpSetting.FromName, smtpSetting.FromEmail));
                message.To.Add(MailboxAddress.Parse(to));
                message.Subject = subject;

                message.Body = new BodyBuilder
                {
                    HtmlBody = body
                }.ToMessageBody();

                using var smtpClient = new MailKit.Net.Smtp.SmtpClient();
                smtpClient.Timeout = SmtpTimeoutMs;

                var socketOptions = smtpSetting.Port switch
                {
                    465 => SecureSocketOptions.SslOnConnect,
                    587 => smtpSetting.EnableSSL ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto,
                    _ => smtpSetting.EnableSSL ? SecureSocketOptions.StartTlsWhenAvailable : SecureSocketOptions.Auto
                };

                await smtpClient.ConnectAsync(smtpSetting.Host, smtpSetting.Port, socketOptions);
                await smtpClient.AuthenticateAsync(smtpSetting.Username, smtpSetting.Password);
                await smtpClient.SendAsync(message);
                await smtpClient.DisconnectAsync(true);
                emailLog.Status = EmailLogStatus.SUCCESS;
            }
            catch (Exception ex)
            {
                emailLog.ErrorMessage = BuildErrorMessage(ex);
                _logger.LogError(ex,
                    "Failed to send email to {ToEmail} via SMTP {Host}:{Port} (SSL={EnableSsl})",
                    to,
                    smtpSetting.Host,
                    smtpSetting.Port,
                    smtpSetting.EnableSSL);
                throw;
            }
            finally
            {
                _dbContext.EmailLogs.Add(emailLog);
                await _dbContext.SaveChangesAsync();
            }
        }

        private static string BuildErrorMessage(Exception exception)
        {
            var baseMessage = exception.GetBaseException().Message;
            var message = string.Equals(baseMessage, exception.Message, StringComparison.Ordinal)
                ? exception.Message
                : $"{exception.Message} | Root cause: {baseMessage}";

            return message.Length <= 2000
                ? message
                : message[..2000];
        }
    }
}
