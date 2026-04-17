using CocoQR.Application.Contracts.IContext;
using CocoQR.Application.Contracts.IQueue;
using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Application.Common.Mapper;
using CocoQR.Application.DTOs.Base.BaseRes;
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
        private readonly IBackgroundJobProducer _backgroundJobProducer;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ILogger<ContactService> _logger;

        public ContactService(
            IUnitOfWork unitOfWork,
            IUserContext userContext,
            IIdGenerator idGenerator,
            IBackgroundJobProducer backgroundJobProducer,
            IEmailService emailService,
            IEmailTemplateService emailTemplateService,
            ILogger<ContactService> logger)
        {
            _unitOfWork = unitOfWork;
            _userContext = userContext;
            _idGenerator = idGenerator;
            _backgroundJobProducer = backgroundJobProducer;
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
                    var thankYouLogId = await CreatePendingEmailLogAsync(
                        request.Email.Trim(),
                        thankYouMail.Subject,
                        thankYouMail.Body,
                        thankYouSmtpSetting.Type,
                        EmailDirection.OUTGOING,
                        EmailTemplateKeys.ContactThankYou);

                    await QueueEmailWithFallbackAsync(
                        request.Email.Trim(),
                        thankYouMail.Subject,
                        thankYouMail.Body,
                        thankYouSmtpSetting,
                        EmailDirection.OUTGOING,
                        EmailTemplateKeys.ContactThankYou,
                        thankYouLogId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to send thank-you email to user for contact message {ContactMessageId}.",
                    message.Id);
            }

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
                    var adminLogId = await CreatePendingEmailLogAsync(
                        adminSmtpSetting.FromEmail,
                        adminMail.Subject,
                        adminMail.Body,
                        adminSmtpSetting.Type,
                        EmailDirection.INCOMING,
                        EmailTemplateKeys.AdminNotify);

                    await QueueEmailWithFallbackAsync(
                        adminSmtpSetting.FromEmail,
                        adminMail.Subject,
                        adminMail.Body,
                        adminSmtpSetting,
                        EmailDirection.INCOMING,
                        EmailTemplateKeys.AdminNotify,
                        adminLogId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to send contact notification to admin for contact message {ContactMessageId}.",
                    message.Id);
            }
        }

        public async Task<PagingVM<GetContactMessageRes>> GetAllAsync(
            int pageNumber,
            int pageSize,
            string? sortField,
            string? sortDirection,
            Guid? userId,
            Guid? providerId,
            string? searchValue,
            bool? isActive,
            ContactMessageStatus? contactStatus,
            DateTime? fromDate,
            DateTime? toDate)
        {
            EnsureAdmin();

            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
            {
                throw new ArgumentException("FromDate phải nhỏ hơn hoặc bằng ToDate.");
            }

            if (pageNumber <= 0)
            {
                pageNumber = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 10;
            }

            var (items, totalCount) = await _unitOfWork.ContactMessages.GetPagedForAdminAsync(
                pageNumber,
                pageSize,
                sortField,
                sortDirection,
                userId,
                providerId,
                searchValue,
                isActive,
                contactStatus,
                fromDate,
                toDate);

            return new PagingVM<GetContactMessageRes>
            {
                List = items.Select(ContactMapper.ToResponse).ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
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

            var smtpSetting = await GetActiveSmtpOrThrowAsync(request.TemplateKey, request.SmtpType);

            var subject = request.Subject.Trim();
            var body = string.IsNullOrWhiteSpace(request.HtmlBody)
                ? request.Content.Trim()
                : request.HtmlBody;

            var emailLogId = await CreatePendingEmailLogAsync(
                request.Email.Trim(),
                subject,
                body,
                smtpSetting.Type,
                EmailDirection.OUTGOING,
                request.TemplateKey);

            await QueueEmailWithFallbackAsync(
                request.Email.Trim(),
                subject,
                body,
                smtpSetting,
                EmailDirection.OUTGOING,
                request.TemplateKey,
                emailLogId);

            if (request.ContactMessageId.HasValue && request.ContactMessageId.Value != Guid.Empty)
            {
                var message = await _unitOfWork.ContactMessages.GetByIdForAdminAsync(request.ContactMessageId.Value)
                    ?? throw new ApplicationException(ErrorCode.NotFound, ErrorMessages.ContactNotFound);

                if (message.Status != ContactMessageStatus.NEW)
                {
                    throw new ApplicationException(
                        ErrorCode.BadRequest,
                        "Chỉ có thể phản hồi liên hệ đang ở trạng thái NEW.");
                }

                message.Status = ContactMessageStatus.REPLIED;
                message.RepliedAt = DateTime.UtcNow;
                await _unitOfWork.ContactMessages.UpdateAsync(message);
            }
        }

        public async Task IgnoreContactMessageAsync(Guid contactMessageId)
        {
            EnsureAdmin();

            if (contactMessageId == Guid.Empty)
            {
                throw new ArgumentException("Id liên hệ không hợp lệ.", nameof(contactMessageId));
            }

            var message = await _unitOfWork.ContactMessages.GetByIdForAdminAsync(contactMessageId)
                ?? throw new ApplicationException(ErrorCode.NotFound, ErrorMessages.ContactNotFound);

            if (message.Status != ContactMessageStatus.NEW)
            {
                throw new ApplicationException(
                    ErrorCode.BadRequest,
                    "Chỉ có thể bỏ qua liên hệ đang ở trạng thái NEW.");
            }

            message.Status = ContactMessageStatus.IGNORED;
            await _unitOfWork.ContactMessages.UpdateAsync(message);
        }

        private void EnsureAdmin()
        {
            if (!_userContext.IsAdmin())
            {
                throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);
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

        private async Task<SmtpSetting> GetActiveSmtpOrThrowAsync(string? templateKey, SmtpSettingType? smtpType = null)
        {
            var resolvedSmtpType = smtpType
                ?? (!string.IsNullOrWhiteSpace(templateKey)
                    ? EmailTemplateSmtpResolver.Resolve(templateKey)
                    : SmtpSettingType.System);

            var smtpSetting = await _unitOfWork.SmtpSettings.GetActiveAsync(resolvedSmtpType);

            if (smtpSetting != null)
                return smtpSetting;

            _logger.LogError(
                "Missing active SMTP setting. TemplateKey={TemplateKey}, RequestedSmtpType={RequestedSmtpType}, ResolvedSmtpType={ResolvedSmtpType}",
                templateKey,
                smtpType,
                resolvedSmtpType);

            throw new ApplicationException(
                ErrorCode.NotFound,
                string.Format(ErrorMessages.SmtpSettingByTypeNotFound, resolvedSmtpType));
        }

        private async Task QueueEmailWithFallbackAsync(
            string to,
            string subject,
            string body,
            SmtpSetting smtpSetting,
            EmailDirection direction,
            string? templateKey,
            Guid emailLogId)
        {
            try
            {
                await _backgroundJobProducer.EnqueueSendEmailAsync(
                    to,
                    subject,
                    body,
                    direction,
                    templateKey,
                    smtpSetting.Type,
                    emailLogId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Queue unavailable, fallback to direct email sending. To={To}, TemplateKey={TemplateKey}",
                    to,
                    templateKey);

                try
                {
                    await _emailService.SendAsync(
                        to,
                        subject,
                        body,
                        smtpSetting,
                        direction,
                        templateKey);

                    await UpdateEmailLogStatusAsync(emailLogId, EmailLogStatus.SUCCESS, null);
                }
                catch (Exception fallbackEx)
                {
                    await UpdateEmailLogStatusAsync(emailLogId, EmailLogStatus.FAIL, fallbackEx.GetBaseException().Message);
                    throw;
                }
            }
        }

        private async Task<Guid> CreatePendingEmailLogAsync(
            string toEmail,
            string subject,
            string body,
            SmtpSettingType smtpType,
            EmailDirection direction,
            string? templateKey)
        {
            var emailLog = new EmailLog
            {
                Id = _idGenerator.NewId(),
                ToEmail = toEmail,
                Subject = subject,
                Body = body,
                Status = EmailLogStatus.PENDING,
                ErrorMessage = null,
                SmtpType = smtpType,
                EmailDirection = direction,
                TemplateKey = templateKey,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.EmailLogs.AddAsync(emailLog);
            return emailLog.Id;
        }

        public async Task UpdateEmailLogStatusAsync(Guid emailLogId, EmailLogStatus status, string? errorMessage)
        {
            var emailLog = await _unitOfWork.EmailLogs.GetByIdAsync(emailLogId);
            if (emailLog == null)
                return;

            emailLog.Status = status;
            emailLog.ErrorMessage = string.IsNullOrWhiteSpace(errorMessage)
                ? null
                : (errorMessage.Length <= 2000 ? errorMessage : errorMessage[..2000]);

            await _unitOfWork.EmailLogs.UpdateAsync(emailLog);
        }

    }
}