using CocoQR.Domain.Entities;
using CocoQR.Domain.Constants.Enum;

namespace CocoQR.Application.Contracts.ISubServices
{
    public interface IEmailService
    {
        Task SendAsync(
            string to,
            string subject,
            string body,
            SmtpSetting smtpSetting,
            EmailDirection direction = EmailDirection.OUTGOING,
            string? templateKey = null);

        Task SendWithoutLogAsync(
            string to,
            string subject,
            string body,
            SmtpSetting smtpSetting,
            EmailDirection direction = EmailDirection.OUTGOING,
            string? templateKey = null);
    }
}
