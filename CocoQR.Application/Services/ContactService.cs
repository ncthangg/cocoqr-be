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

            var senderUser = await _unitOfWork.Users.GetByEmailAsync(message.Email);
            var recipientUser = string.IsNullOrWhiteSpace(_emailConfiguration.AdminNotificationEmail)
                ? null
                : await _unitOfWork.Users.GetByEmailAsync(_emailConfiguration.AdminNotificationEmail);
            var conversation = new EmailConversation
            {
                Id = _idGenerator.NewId(),
                ContactMessageId = message.Id,
                InitiatorUserId = senderUser?.Id,
                RecipientUserId = recipientUser?.Id,
                InitiatorEmail = message.Email,
                RecipientEmail = _emailConfiguration.AdminNotificationEmail ?? "cocoqr",
                Subject = "Lien he moi tu nguoi dung",
                CreatedAt = message.CreatedAt,
                LastMessageAt = message.CreatedAt
            };
            await _unitOfWork.EmailConversations.AddAsync(conversation);

            await AddConversationMessageAsync(new EmailConversationMessage
            {
                ConversationId = conversation.Id,
                SenderUserId = senderUser?.Id,
                FromEmail = message.Email,
                ToEmail = _emailConfiguration.AdminNotificationEmail ?? "cocoqr",
                Subject = "Lien he moi tu nguoi dung",
                Body = message.Content,
                Direction = EmailDirection.INBOUND,
                Status = EmailDeliveryStatus.RECEIVED
            });

            var thankYouMail = await ResolveTemplateOrDefaultAsync(
                EmailTemplateKeys.ContactThankYou,
                request,
                "Cam on ban da lien he",
                $"<p>Xin chao {WebUtility.HtmlEncode(request.FullName.Trim())},</p><p>Cam on ban da lien he voi he thong. Chung toi da nhan duoc thong tin va se phan hoi som nhat co the.</p>");

            var thankYouResult = await SendEmailBestEffortAsync(
                request.Email.Trim(),
                thankYouMail.Subject,
                thankYouMail.Body,
                "Failed to send thank-you email for contact message {ContactMessageId}.",
                message.Id);

            await AddConversationMessageAsync(new EmailConversationMessage
            {
                ConversationId = conversation.Id,
                RecipientUserId = senderUser?.Id,
                FromEmail = _emailConfiguration.AdminNotificationEmail ?? "cocoqr",
                ToEmail = request.Email.Trim(),
                Subject = thankYouMail.Subject,
                Body = thankYouMail.Body,
                Direction = EmailDirection.OUTBOUND,
                Status = thankYouResult.Error == null
                    ? MapAcceptedStatus(thankYouResult.Response?.Data?.Status)
                    : EmailDeliveryStatus.FAILED,
                GatewayMessageId = thankYouResult.Response?.Data?.EmailMessageId,
                CorrelationId = thankYouResult.Response?.Data?.CorrelationId,
                ErrorMessage = thankYouResult.Error?.Message
            });

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

            var (items, totalCount) = await _unitOfWork.EmailConversations.GetPagedForAdminAsync(
                pageNumber,
                pageSize,
                sortField,
                sortDirection,
                userId,
                searchValue,
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

        public async Task<IEnumerable<EmailConversationMessageRes>> GetConversationAsync(
            Guid conversationId)
        {
            EnsureAdmin();

            if (conversationId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Id conversation khong hop le.",
                    nameof(conversationId));
            }

            _ = await _unitOfWork.EmailConversations.GetByIdAsync(conversationId)
                ?? throw new ApplicationException(
                    ErrorCode.NotFound,
                    "Khong tim thay conversation.");

            var messages = await _unitOfWork.EmailConversationMessages
                .GetByConversationIdAsync(conversationId);

            return messages.Select(x => new EmailConversationMessageRes
            {
                Id = x.Id,
                ConversationId = x.ConversationId,
                SequenceNumber = x.SequenceNumber,
                SenderUserId = x.SenderUserId,
                RecipientUserId = x.RecipientUserId,
                FromEmail = x.FromEmail,
                ToEmail = x.ToEmail,
                Subject = x.Subject,
                Body = x.Body,
                Direction = x.Direction,
                Status = x.Status,
                GatewayMessageId = x.GatewayMessageId,
                CorrelationId = x.CorrelationId,
                LastCallbackEventId = x.LastCallbackEventId,
                LastCallbackAt = x.LastCallbackAt,
                FailureCode = x.FailureCode,
                ProviderMessageId = x.ProviderMessageId,
                ErrorMessage = x.ErrorMessage,
                CreatedAt = x.CreatedAt
            });
        }

        public async Task<SendEmailConversationMessageRes> ContactFromSystemAsync(
            AdminContactRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            EnsureAdmin();
            ValidateAdminRequest(request);

            var subject = request.Subject.Trim();
            var body = string.IsNullOrWhiteSpace(request.HtmlBody)
                ? request.Content.Trim()
                : request.HtmlBody;

            var senderUserId = _userContext.UserId
                ?? throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);
            var senderUser = await _unitOfWork.Users.GetByIdAsync(senderUserId)
                ?? throw new ApplicationException(ErrorCode.NotFound, ErrorMessages.UserNotFound);
            var recipientUser = await _unitOfWork.Users.GetByEmailAsync(request.Email.Trim());

            var (conversation, contactMessage) = await ResolveConversationAsync(
                request,
                senderUser,
                recipientUser,
                subject);

            MailGatewaySendResponse? sendResponse = null;
            Exception? sendError = null;
            try
            {
                sendResponse = await _emailService.SendAsync(request.Email.Trim(), subject, body);
            }
            catch (Exception ex)
            {
                sendError = ex;
            }

            var conversationMessage = new EmailConversationMessage
            {
                ConversationId = conversation.Id,
                SenderUserId = senderUser.Id,
                RecipientUserId = recipientUser?.Id,
                FromEmail = senderUser.Email,
                ToEmail = request.Email.Trim(),
                Subject = subject,
                Body = body,
                Direction = EmailDirection.OUTBOUND,
                Status = sendError == null
                    ? MapAcceptedStatus(sendResponse?.Data?.Status)
                    : EmailDeliveryStatus.FAILED,
                GatewayMessageId = sendResponse?.Data?.EmailMessageId,
                CorrelationId = sendResponse?.Data?.CorrelationId,
                ErrorMessage = sendError?.Message
            };
            var sequenceNumber = await AddConversationMessageAsync(conversationMessage);

            if (sendError != null)
            {
                throw sendError;
            }

            if (contactMessage != null)
            {
                contactMessage.Status = ContactMessageStatus.REPLIED;
                contactMessage.RepliedAt = DateTime.UtcNow;
                await _unitOfWork.ContactMessages.UpdateAsync(contactMessage);
            }

            return new SendEmailConversationMessageRes
            {
                ConversationId = conversation.Id,
                MessageId = conversationMessage.Id,
                SequenceNumber = sequenceNumber
            };
        }

        public async Task DeleteConversationAsync(Guid conversationId)
        {
            EnsureAdmin();

            if (conversationId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Id conversation khong hop le.",
                    nameof(conversationId));
            }

            _ = await _unitOfWork.EmailConversations.GetByIdAsync(conversationId)
                ?? throw new ApplicationException(
                    ErrorCode.NotFound,
                    "Khong tim thay conversation.");

            await _unitOfWork.EmailConversations.DeleteAsync(conversationId);
        }

        public async Task DeleteConversationMessageAsync(Guid conversationId, Guid messageId)
        {
            EnsureAdmin();

            if (conversationId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Id conversation khong hop le.",
                    nameof(conversationId));
            }

            if (messageId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Id message khong hop le.",
                    nameof(messageId));
            }

            var conversation = await _unitOfWork.EmailConversations.GetByIdAsync(conversationId)
                ?? throw new ApplicationException(
                    ErrorCode.NotFound,
                    "Khong tim thay conversation.");

            var message = await _unitOfWork.EmailConversationMessages.GetByIdAsync(messageId)
                ?? throw new ApplicationException(
                    ErrorCode.NotFound,
                    "Khong tim thay message trong conversation.");

            if (message.ConversationId != conversationId)
            {
                throw new ApplicationException(
                    ErrorCode.BadRequest,
                    "Message khong thuoc conversation nay.");
            }

            await _unitOfWork.EmailConversationMessages.DeleteAsync(messageId);

            var remainingMessages = await _unitOfWork.EmailConversationMessages
                .GetByConversationIdAsync(conversationId);
            var lastMessageAt = remainingMessages.LastOrDefault()?.CreatedAt ?? conversation.CreatedAt;

            await _unitOfWork.EmailConversations
                .UpdateLastMessageAtAsync(conversationId, lastMessageAt);
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

        private async Task<(MailGatewaySendResponse? Response, Exception? Error)> SendEmailBestEffortAsync(
            string to,
            string subject,
            string body,
            string logMessage,
            Guid contactMessageId)
        {
            try
            {
                var response = await _emailService.SendAsync(to, subject, body);
                return (response, null);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, logMessage, contactMessageId);
                return (null, ex);
            }
        }

        private async Task<(EmailConversation Conversation, ContactMessage? ContactMessage)>
            ResolveConversationAsync(
                AdminContactRequest request,
                User senderUser,
                User? recipientUser,
                string subject)
        {
            ContactMessage? contactMessage = null;
            EmailConversation? conversation = null;

            if (request.ConversationId.HasValue && request.ConversationId.Value != Guid.Empty)
            {
                conversation = await _unitOfWork.EmailConversations
                    .GetByIdAsync(request.ConversationId.Value)
                    ?? throw new ApplicationException(
                        ErrorCode.NotFound,
                        "Khong tim thay conversation.");
            }
            else if (request.ContactMessageId.HasValue &&
                     request.ContactMessageId.Value != Guid.Empty)
            {
                contactMessage = await GetReplyableContactMessageAsync(request.ContactMessageId.Value);
                conversation = await _unitOfWork.EmailConversations
                    .GetByContactMessageIdAsync(contactMessage.Id)
                    ?? throw new ApplicationException(
                        ErrorCode.NotFound,
                        "Khong tim thay conversation cua lien he.");
            }

            if (conversation != null)
            {
                var expectedRecipientEmail =
                    conversation.InitiatorUserId == senderUser.Id
                        ? conversation.RecipientEmail
                        : conversation.RecipientUserId == senderUser.Id ||
                          conversation.ContactMessageId.HasValue
                            ? conversation.InitiatorEmail
                            : conversation.RecipientEmail;

                if (!string.Equals(
                        expectedRecipientEmail,
                        request.Email.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new ApplicationException(
                        ErrorCode.BadRequest,
                        "Nguoi nhan khong dung voi conversation nay.");
                }

                if (conversation.ContactMessageId.HasValue && contactMessage == null)
                {
                    contactMessage = await GetReplyableContactMessageAsync(
                        conversation.ContactMessageId.Value);
                }

                return (conversation, contactMessage);
            }

            if (recipientUser == null)
            {
                throw new ApplicationException(
                    ErrorCode.NotFound,
                    "Nguoi nhan chua ton tai trong he thong.");
            }

            var now = DateTime.UtcNow;
            conversation = new EmailConversation
            {
                Id = _idGenerator.NewId(),
                InitiatorUserId = senderUser.Id,
                RecipientUserId = recipientUser.Id,
                InitiatorEmail = senderUser.Email,
                RecipientEmail = recipientUser.Email,
                Subject = subject,
                CreatedAt = now,
                LastMessageAt = now
            };
            await _unitOfWork.EmailConversations.AddAsync(conversation);
            return (conversation, null);
        }

        private async Task<ContactMessage> GetReplyableContactMessageAsync(Guid contactMessageId)
        {
            var contactMessage = await _unitOfWork.ContactMessages
                .GetByIdForAdminAsync(contactMessageId)
                ?? throw new ApplicationException(ErrorCode.NotFound, ErrorMessages.ContactNotFound);

            if (contactMessage.Status == ContactMessageStatus.IGNORED)
            {
                throw new ApplicationException(
                    ErrorCode.BadRequest,
                    "Khong the phan hoi lien he da bi bo qua.");
            }

            return contactMessage;
        }

        private async Task<int> AddConversationMessageAsync(EmailConversationMessage message)
        {
            message.Id = _idGenerator.NewId();
            message.CreatedAt = DateTime.UtcNow;
            if (message.ErrorMessage?.Length > 2000)
            {
                message.ErrorMessage = message.ErrorMessage[..2000];
            }

            var sequenceNumber = await _unitOfWork.EmailConversationMessages
                .AddToConversationAsync(message);
            await _unitOfWork.EmailConversations
                .UpdateLastMessageAtAsync(message.ConversationId, message.CreatedAt);
            return sequenceNumber;
        }

        private static EmailDeliveryStatus MapAcceptedStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return EmailDeliveryStatus.QUEUED;
            }

            return status.Trim().ToUpperInvariant() switch
            {
                "SENT" => EmailDeliveryStatus.SENT,
                "FAILED" => EmailDeliveryStatus.FAILED,
                "QUEUED" => EmailDeliveryStatus.QUEUED,
                "PENDING" => EmailDeliveryStatus.PENDING,
                "SENDING" => EmailDeliveryStatus.SENDING,
                "CANCELLED" => EmailDeliveryStatus.CANCELLED,
                _ => EmailDeliveryStatus.QUEUED
            };
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
