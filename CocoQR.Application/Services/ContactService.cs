using CocoQR.Application.Contracts.IContext;
using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Application.Common.Mapper;
using CocoQR.Application.DTOs.Contacts.Requests;
using CocoQR.Application.DTOs.Contacts.Responses;
using CocoQR.Domain.Constants;
using CocoQR.Domain.Constants.Enum;
using CocoQR.Domain.Entities;
using CocoQR.Domain.Helper;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using ApplicationException = CocoQR.Application.Exceptions.ApplicationException;

namespace CocoQR.Application.Services
{
    public class ContactService : IContactService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly IIdGenerator _idGenerator;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ILogger<ContactService> _logger;

        public ContactService(
            IUnitOfWork unitOfWork,
            IUserContext userContext,
            IIdGenerator idGenerator,
            IEmailService emailService,
            IEmailTemplateService emailTemplateService,
            ILogger<ContactService> logger)
        {
            _unitOfWork = unitOfWork;
            _userContext = userContext;
            _idGenerator = idGenerator;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
            _logger = logger;
        }

        public async Task ContactToSystemAsync(ContactRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            ValidatePublicRequest(request);

            var message = new ContactMessage
            {
                Id = _idGenerator.NewId(),
                FullName = request.FullName.Trim(),
                Email = request.Email.Trim(),
                Content = request.Content.Trim(),
                Status = ContactMessageStatus.NEW,
                CreatedAt = DateTime.UtcNow,
                RepliedAt = null
            };

            // Save contact payload first, then send email.
            await _unitOfWork.ContactMessages.AddAsync(message);

            try
            {
                var adminSmtpType = EmailTemplateSmtpResolver.Resolve(EmailTemplateKeys.AdminNotify);
                var adminSmtpSetting = await _unitOfWork.SmtpSettings.GetActiveAsync(adminSmtpType);
                if (adminSmtpSetting == null)
                {
                    await SaveFailedEmailLogAsync(
                        smtpType: adminSmtpType,
                        toEmail: "admin-contact@system.local",
                        subject: "Liên hệ mới từ người dùng",
                        body: request.Content.Trim(),
                        direction: EmailDirection.INCOMING,
                        templateKey: EmailTemplateKeys.AdminNotify,
                        errorMessage: $"Không tìm thấy SMTP active cho {adminSmtpType}.");

                    _logger.LogWarning("No active SMTP setting found for template {TemplateKey} resolved type {SmtpType}.",
                        EmailTemplateKeys.AdminNotify,
                        adminSmtpType);
                }

                var adminMail = await ResolveTemplateOrDefaultAsync(
                    EmailTemplateKeys.AdminNotify,
                    request,
                    "Liên hệ mới từ người dùng",
                    $"<p><strong>Người gửi:</strong> {WebUtility.HtmlEncode(request.FullName.Trim())} ({WebUtility.HtmlEncode(request.Email.Trim())})</p><p><strong>Nội dung:</strong></p><p>{WebUtility.HtmlEncode(request.Content.Trim()).Replace("\r\n", "<br/>").Replace("\n", "<br/>")}</p>");

                if (adminSmtpSetting != null)
                {
                    await _emailService.SendAsync(
                        adminSmtpSetting.FromEmail,
                        adminMail.Subject,
                        adminMail.Body,
                        adminSmtpSetting,
                        EmailDirection.INCOMING,
                        EmailTemplateKeys.AdminNotify);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to send contact notification to admin for contact message {ContactMessageId}.",
                    message.Id);
            }

            try
            {
                var thankYouSmtpType = EmailTemplateSmtpResolver.Resolve(EmailTemplateKeys.ContactThankYou);
                var thankYouSmtpSetting = await _unitOfWork.SmtpSettings.GetActiveAsync(thankYouSmtpType);
                if (thankYouSmtpSetting == null)
                {
                    await SaveFailedEmailLogAsync(
                        smtpType: thankYouSmtpType,
                        toEmail: request.Email.Trim(),
                        subject: "Cảm ơn bạn đã liên hệ",
                        body: request.Content.Trim(),
                        direction: EmailDirection.OUTGOING,
                        templateKey: EmailTemplateKeys.ContactThankYou,
                        errorMessage: $"Không tìm thấy SMTP active cho {thankYouSmtpType}.");

                    _logger.LogWarning("No active SMTP setting found for template {TemplateKey} resolved type {SmtpType}.",
                        EmailTemplateKeys.ContactThankYou,
                        thankYouSmtpType);
                }

                var thankYouMail = await ResolveTemplateOrDefaultAsync(
                    EmailTemplateKeys.ContactThankYou,
                    request,
                    "Cảm ơn bạn đã liên hệ",
                    $"<p>Xin chào {WebUtility.HtmlEncode(request.FullName.Trim())},</p><p>Cảm ơn bạn đã liên hệ với hệ thống. Chúng tôi đã nhận được thông tin và sẽ phản hồi sớm nhất có thể.</p>");

                if (thankYouSmtpSetting != null)
                {
                    await _emailService.SendAsync(
                        request.Email.Trim(),
                        thankYouMail.Subject,
                        thankYouMail.Body,
                        thankYouSmtpSetting,
                        EmailDirection.OUTGOING,
                        EmailTemplateKeys.ContactThankYou);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to send thank-you email to user for contact message {ContactMessageId}.",
                    message.Id);
            }
        }

        public async Task<IEnumerable<GetContactMessageRes>> GetAllAsync()
        {
            EnsureAdmin();

            var messages = await _unitOfWork.ContactMessages.GetAllForAdminAsync();

            return messages.Select(ContactMapper.ToResponse).ToList();
        }

        public async Task<GetContactMessageRes> GetByIdAsync(Guid id)
        {
            EnsureAdmin();

            if (id == Guid.Empty)
            {
                throw new ArgumentException("Id liên hệ không hợp lệ.", nameof(id));
            }

            var message = await _unitOfWork.ContactMessages.GetByIdForAdminAsync(id)
                ?? throw new ApplicationException(ErrorCode.NotFound, ErrorMessages.ContactNotFound);

            return ContactMapper.ToResponse(message);
        }

        public async Task ContactFromSystemAsync(AdminContactRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            EnsureAdmin();
            ValidateAdminRequest(request);

            var selectedSmtpType = EmailTemplateSmtpResolver.Resolve(request.TemplateKey);
            var smtpSetting = await _unitOfWork.SmtpSettings.GetActiveAsync(selectedSmtpType)
                ?? throw new ApplicationException(
                    ErrorCode.NotFound,
                    string.Format(ErrorMessages.SmtpSettingByTypeNotFound, selectedSmtpType));

            var rendered = await ResolveAdminTemplateOrDefaultAsync(request);

            await _emailService.SendAsync(
                request.Email.Trim(),
                rendered.Subject,
                rendered.Body,
                smtpSetting,
                EmailDirection.OUTGOING,
                request.TemplateKey);
        }

        private void EnsureAdmin()
        {
            if (!_userContext.IsAdmin())
            {
                throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);
            }
        }

        private static string BuildSystemToUserBody(AdminContactRequest request)
        {
            var receiverName = WebUtility.HtmlEncode(request.FullName.Trim());
            var content = string.IsNullOrWhiteSpace(request.HtmlBody)
                ? request.Content.Trim()
                : request.HtmlBody;
            var encodedContent = WebUtility.HtmlEncode(content)
                .Replace("\r\n", "<br/>")
                .Replace("\n", "<br/>");

            return $"<p>Xin chào {receiverName},</p><p>{encodedContent}</p>";
        }

        private async Task<(string Subject, string Body)> ResolveAdminTemplateOrDefaultAsync(AdminContactRequest request)
        {
            var variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["FullName"] = request.FullName.Trim(),
                ["Email"] = request.Email.Trim(),
                ["Subject"] = request.Subject.Trim(),
                ["Content"] = request.Content.Trim(),
                ["Body"] = request.Content.Trim()
            };

            try
            {
                var rendered = await _emailTemplateService.RenderAsync(request.TemplateKey.Trim(), variables);
                if (!string.IsNullOrWhiteSpace(request.HtmlBody))
                {
                    return (rendered.Subject, request.HtmlBody);
                }

                return rendered;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to render email template {TemplateKey}. Fallback to default content for admin flow.",
                    request.TemplateKey);

                return (request.Subject.Trim(), BuildSystemToUserBody(request));
            }
        }

        private async Task<(string Subject, string Body)> ResolveTemplateOrDefaultAsync(
            string templateKey,
            ContactRequest request,
            string defaultSubject,
            string defaultBody)
        {
            var variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["FullName"] = request.FullName.Trim(),
                ["Email"] = request.Email.Trim(),
                ["Subject"] = "Liên hệ mới từ người dùng",
                ["Content"] = request.Content.Trim(),
                ["Body"] = request.Content.Trim()
            };

            try
            {
                return await _emailTemplateService.RenderAsync(templateKey, variables);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to render email template {TemplateKey}. Fallback to default content.",
                    templateKey);

                return (defaultSubject, defaultBody);
            }
        }

        private static void ValidatePublicRequest(ContactRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                throw new ArgumentException("FullName là bắt buộc.", nameof(request.FullName));
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new ArgumentException(ValidationMessages.RequiredEmail, nameof(request.Email));
            }

            if (!IsValidEmail(request.Email.Trim()))
            {
                throw new ArgumentException(ValidationMessages.InvalidEmail, nameof(request.Email));
            }

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                throw new ArgumentException(ValidationMessages.RequiredBody, nameof(request.Content));
            }
        }

        private static void ValidateAdminRequest(AdminContactRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                throw new ArgumentException("FullName là bắt buộc.", nameof(request.FullName));
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new ArgumentException(ValidationMessages.RequiredEmail, nameof(request.Email));
            }

            if (!IsValidEmail(request.Email.Trim()))
            {
                throw new ArgumentException(ValidationMessages.InvalidEmail, nameof(request.Email));
            }

            if (string.IsNullOrWhiteSpace(request.Subject))
            {
                throw new ArgumentException(ValidationMessages.RequiredSubject, nameof(request.Subject));
            }

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                throw new ArgumentException("Content là bắt buộc.", nameof(request.Content));
            }

            if (string.IsNullOrWhiteSpace(request.TemplateKey))
            {
                throw new ArgumentException("Template key là bắt buộc.", nameof(request.TemplateKey));
            }
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var parsed = new MailAddress(email);
                return parsed.Address.Equals(email, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private async Task SaveFailedEmailLogAsync(
            SmtpSettingType smtpType,
            string toEmail,
            string subject,
            string body,
            EmailDirection direction,
            string? templateKey,
            string errorMessage)
        {
            await _unitOfWork.EmailLogs.AddAsync(new EmailLog
            {
                Id = _idGenerator.NewId(),
                ToEmail = toEmail,
                Subject = subject,
                Body = body,
                Status = EmailLogStatus.FAIL,
                ErrorMessage = errorMessage,
                SmtpType = smtpType,
                EmailDirection = direction,
                TemplateKey = templateKey,
                CreatedAt = DateTime.UtcNow
            });
        }

    }
}