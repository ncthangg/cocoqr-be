using CocoQR.Application.Common.Mapper;
using CocoQR.Application.Contracts.IConfigs;
using CocoQR.Application.Contracts.IContext;
using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Application.DTOs.Contacts.Requests;
using CocoQR.Application.DTOs.Contacts.Responses;
using CocoQR.Domain.Constants;
using CocoQR.Domain.Constants.Enum;
using CocoQR.Domain.Entities;
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
        private readonly IEmailConfiguration _emailConfiguration;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ILogger<ContactService> _logger;

        public ContactService(
            IUnitOfWork unitOfWork,
            IUserContext userContext,
            IIdGenerator idGenerator,
            IEmailConfiguration emailConfiguration,
            IEmailService emailService,
            IEmailTemplateService emailTemplateService,
            ILogger<ContactService> logger)
        {
            _unitOfWork = unitOfWork;
            _userContext = userContext;
            _idGenerator = idGenerator;
            _emailConfiguration = emailConfiguration;
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

            await _unitOfWork.ContactMessages.AddAsync(message);

            var thankYouMail = await ResolveTemplateOrDefaultAsync(
                EmailTemplateKeys.ContactThankYou,
                request,
                "Cam on ban da lien he",
                $"<p>Xin chao {WebUtility.HtmlEncode(request.FullName.Trim())},</p><p>Cam on ban da lien he voi he thong. Chung toi da nhan duoc thong tin va se phan hoi som nhat co the.</p>");

            await SendEmailBestEffortAsync(
                request.Email.Trim(),
                thankYouMail.Subject,
                thankYouMail.Body,
                "Failed to send thank-you email for contact message {ContactMessageId}.",
                message.Id);

            var adminMail = await ResolveTemplateOrDefaultAsync(
                EmailTemplateKeys.AdminNotify,
                request,
                "Lien he moi tu nguoi dung",
                $"<p><strong>Nguoi gui:</strong> {WebUtility.HtmlEncode(request.FullName.Trim())} ({WebUtility.HtmlEncode(request.Email.Trim())})</p><p><strong>Noi dung:</strong></p><p>{WebUtility.HtmlEncode(request.Content.Trim()).Replace("\r\n", "<br/>").Replace("\n", "<br/>")}</p>");

            if (!string.IsNullOrWhiteSpace(_emailConfiguration.AdminNotificationEmail))
            {
                await SendEmailBestEffortAsync(
                    _emailConfiguration.AdminNotificationEmail,
                    adminMail.Subject,
                    adminMail.Body,
                    "Failed to send contact notification for contact message {ContactMessageId}.",
                    message.Id);
            }
            else
            {
                _logger.LogWarning(
                    "CocoMail:AdminNotificationEmail is not configured. Skip admin notification for contact message {ContactMessageId}.",
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
                throw new ArgumentException("FromDate phai nho hon hoac bang ToDate.");
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
                throw new ArgumentException("Id lien he khong hop le.", nameof(id));
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

            var subject = request.Subject.Trim();
            var body = string.IsNullOrWhiteSpace(request.HtmlBody)
                ? request.Content.Trim()
                : request.HtmlBody;

            await _emailService.SendAsync(request.Email.Trim(), subject, body);

            if (request.ContactMessageId.HasValue && request.ContactMessageId.Value != Guid.Empty)
            {
                var message = await _unitOfWork.ContactMessages.GetByIdForAdminAsync(request.ContactMessageId.Value)
                    ?? throw new ApplicationException(ErrorCode.NotFound, ErrorMessages.ContactNotFound);

                if (message.Status != ContactMessageStatus.NEW)
                {
                    throw new ApplicationException(
                        ErrorCode.BadRequest,
                        "Chi co the phan hoi lien he dang o trang thai NEW.");
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
                throw new ArgumentException("Id lien he khong hop le.", nameof(contactMessageId));
            }

            var message = await _unitOfWork.ContactMessages.GetByIdForAdminAsync(contactMessageId)
                ?? throw new ApplicationException(ErrorCode.NotFound, ErrorMessages.ContactNotFound);

            if (message.Status != ContactMessageStatus.NEW)
            {
                throw new ApplicationException(
                    ErrorCode.BadRequest,
                    "Chi co the bo qua lien he dang o trang thai NEW.");
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
                ["Subject"] = "Lien he moi tu nguoi dung",
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

        private async Task SendEmailBestEffortAsync(
            string to,
            string subject,
            string body,
            string logMessage,
            Guid contactMessageId)
        {
            try
            {
                await _emailService.SendAsync(to, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, logMessage, contactMessageId);
            }
        }

        private static void ValidatePublicRequest(ContactRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                throw new ArgumentException("FullName la bat buoc.", nameof(request.FullName));
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
                throw new ArgumentException("FullName la bat buoc.", nameof(request.FullName));
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
                throw new ArgumentException("Content la bat buoc.", nameof(request.Content));
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
    }
}
