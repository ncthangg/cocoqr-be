using CocoQR.Application.Contracts.IContext;
using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Application.DTOs.Settings;
using CocoQR.Domain.Constants;
using CocoQR.Domain.Constants.Enum;
using CocoQR.Domain.Entities;
using System.Net.Sockets;
using ApplicationException = CocoQR.Application.Exceptions.ApplicationException;
using DomainException = CocoQR.Domain.Exceptions.DomainException;

namespace CocoQR.Application.Services
{
    public class SmtpSettingService : ISmtpSettingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly IIdGenerator _idGenerator;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _emailTemplateService;

        public SmtpSettingService(
            IUnitOfWork unitOfWork,
            IUserContext userContext,
            IIdGenerator idGenerator,
            IEmailService emailService,
            IEmailTemplateService emailTemplateService)
        {
            _unitOfWork = unitOfWork;
            _userContext = userContext;
            _idGenerator = idGenerator;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
        }

        public async Task<IEnumerable<GetSmtpSettingRes>> GetAsync(GetSmtpSettingReq request)
        {
            EnsureAdmin();
            ArgumentNullException.ThrowIfNull(request);

            if (request.Type.HasValue)
            {
                ValidateType(request.Type.Value);
            }

            var settings = (await _unitOfWork.SmtpSettings.GetAsync(request.Type)).ToList();

            if (request.Type.HasValue && settings.Count == 0)
            {
                throw new ApplicationException(
                    ErrorCode.NotFound,
                    string.Format(ErrorMessages.SmtpSettingByTypeNotFound, request.Type.Value));
            }

            return settings.Select(ToResponse).ToList();
        }

        public async Task<GetSmtpSettingRes> PutAsync(PutSmtpSettingReq request)
        {
            ArgumentNullException.ThrowIfNull(request);

            EnsureAdmin();

            var setting = await _unitOfWork.SmtpSettings.GetByTypeAsync(request.Type);
            var isCreate = setting == null;

            ValidatePutRequest(request, isCreate);

            if (isCreate)
            {
                setting = new SmtpSetting
                {
                    Id = _idGenerator.NewId(),
                    Host = request.Host.Trim(),
                    Port = request.Port,
                    Username = request.Username.Trim(),
                    Password = request.Password,
                    EnableSSL = request.EnableSSL,
                    FromEmail = request.FromEmail.Trim(),
                    FromName = request.FromName.Trim(),
                    Type = request.Type,
                    IsActive = request.IsActive,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.SmtpSettings.AddAsync(setting);
            }
            else
            {
                var existingSetting = setting!;

                existingSetting.Host = request.Host.Trim();
                existingSetting.Port = request.Port;
                existingSetting.Username = request.Username.Trim();
                if (!string.IsNullOrWhiteSpace(request.Password))
                {
                    existingSetting.Password = request.Password;
                }
                existingSetting.EnableSSL = request.EnableSSL;
                existingSetting.FromEmail = request.FromEmail.Trim();
                existingSetting.FromName = request.FromName.Trim();
                existingSetting.Type = request.Type;
                existingSetting.IsActive = request.IsActive;
                existingSetting.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SmtpSettings.UpdateAsync(existingSetting);
            }

            return ToResponse(setting!);
        }

        public async Task DeleteAsync(Guid id)
        {
            EnsureAdmin();
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Invalid ID", nameof(id));
            }

            var setting = await _unitOfWork.SmtpSettings.GetByIdAsync(id)
                ?? throw new ApplicationException(
                    ErrorCode.NotFound,
                    ErrorMessages.EntityNotFound);

            await _unitOfWork.SmtpSettings.DeleteAsync(id);
        }

        public async Task TestAsync(TestSmtpSettingReq request)
        {
            ArgumentNullException.ThrowIfNull(request);

            EnsureAdmin();

            if (string.IsNullOrWhiteSpace(request.ToEmail))
            {
                throw new ArgumentException(ValidationMessages.RequiredToEmail, nameof(request.ToEmail));
            }

            ValidateType(request.Type);

            var smtpSetting = await _unitOfWork.SmtpSettings.GetByTypeAsync(request.Type)
                ?? throw new ApplicationException(
                    ErrorCode.NotFound,
                    string.Format(ErrorMessages.SmtpSettingByTypeNotFound, request.Type));

            EnsureSmtpSettingIsActiveForSending(smtpSetting, request.Type);

            string subject;
            string body;

            if (!string.IsNullOrWhiteSpace(request.TemplateKey))
            {
                var rendered = await _emailTemplateService.RenderAsync(request.TemplateKey.Trim(), request.Variables);
                subject = rendered.Subject;
                body = rendered.Body;
            }
            else
            {
                subject = request.Subject?.Trim() ?? string.Empty;
                body = request.Body ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentException(ValidationMessages.RequiredSubject, nameof(subject));
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                throw new ArgumentException(ValidationMessages.RequiredBody, nameof(body));
            }

            try
            {
                await _emailService.SendAsync(request.ToEmail.Trim(), subject, body, smtpSetting);
            }
            catch (TimeoutException ex)
            {
                throw new ApplicationException(
                    ErrorCode.ServiceUnavailable,
                    string.Format(ErrorMessages.SmtpConnectionFailed, smtpSetting.Host, smtpSetting.Port),
                    data: new
                    {
                        smtpSetting.Host,
                        smtpSetting.Port,
                        smtpSetting.EnableSSL,
                        Error = ex.Message
                    },
                    innerException: ex);
            }
            catch (Exception ex) when (TryGetSocketException(ex, out var socketEx) && socketEx?.SocketErrorCode == SocketError.TimedOut)
            {
                throw new ApplicationException(
                    ErrorCode.ServiceUnavailable,
                    string.Format(ErrorMessages.SmtpConnectionFailed, smtpSetting.Host, smtpSetting.Port),
                    data: new
                    {
                        smtpSetting.Host,
                        smtpSetting.Port,
                        smtpSetting.EnableSSL,
                        SocketError = socketEx?.SocketErrorCode.ToString()
                    },
                        innerException: ex);
            }
            catch (Exception ex) when (TryGetSocketException(ex, out _))
            {
                throw new ApplicationException(
                    ErrorCode.ServiceUnavailable,
                    string.Format(ErrorMessages.SmtpConnectionFailed, smtpSetting.Host, smtpSetting.Port),
                    data: new
                    {
                        smtpSetting.Host,
                        smtpSetting.Port,
                        smtpSetting.EnableSSL,
                        Error = ex.GetBaseException().Message
                    },
                        innerException: ex);
            }
            catch (Exception ex) when (IsSmtpCommandFailure(ex))
            {
                throw new ApplicationException(
                    ErrorCode.ServiceUnavailable,
                    string.Format(ErrorMessages.SmtpSendFailed, smtpSetting.Host, smtpSetting.Port),
                    data: new
                    {
                        smtpSetting.Host,
                        smtpSetting.Port,
                        smtpSetting.EnableSSL,
                        Error = ex.GetBaseException().Message
                    },
                    innerException: ex);
            }
            catch (Exception ex) when (IsSmtpProtocolFailure(ex))
            {
                throw new ApplicationException(
                    ErrorCode.ServiceUnavailable,
                    string.Format(ErrorMessages.SmtpProtocolFailed, smtpSetting.Host, smtpSetting.Port),
                    data: new
                    {
                        smtpSetting.Host,
                        smtpSetting.Port,
                        smtpSetting.EnableSSL,
                        Error = ex.GetBaseException().Message
                    },
                    innerException: ex);
            }
            catch (Exception ex) when (IsAuthenticationFailure(ex))
            {
                throw new ApplicationException(
                    ErrorCode.ServiceUnavailable,
                    string.Format(ErrorMessages.SmtpAuthenticationFailed, smtpSetting.Host, smtpSetting.Port),
                    data: new
                    {
                        smtpSetting.Host,
                        smtpSetting.Port,
                        smtpSetting.EnableSSL,
                        Error = ex.GetBaseException().Message
                    },
                    innerException: ex);
            }
        }

        private static bool TryGetSocketException(Exception exception, out SocketException? socketException)
        {
            socketException = exception as SocketException;

            if (socketException != null)
            {
                return true;
            }

            if (exception.InnerException is SocketException innerSocketException)
            {
                socketException = innerSocketException;
                return true;
            }

            return false;
        }

        private static bool IsSmtpCommandFailure(Exception exception)
        {
            return exception.GetType().Name == "SmtpCommandException";
        }

        private static bool IsSmtpProtocolFailure(Exception exception)
        {
            return exception.GetType().Name == "SmtpProtocolException";
        }

        private static bool IsAuthenticationFailure(Exception exception)
        {
            return exception.GetType().Name == "AuthenticationException"
                && exception.GetType().Namespace?.Contains("MailKit", StringComparison.OrdinalIgnoreCase) == true;
        }

        private void EnsureAdmin()
        {
            if (!_userContext.IsAdmin())
            {
                throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);
            }
        }

        private static void ValidatePutRequest(PutSmtpSettingReq request, bool isCreate)
        {
            ValidateType(request.Type);

            if (string.IsNullOrWhiteSpace(request.Host))
            {
                throw new ArgumentException(ValidationMessages.RequiredHost, nameof(request.Host));
            }

            if (request.Port <= 0)
            {
                throw new ArgumentException(ValidationMessages.InvalidPort, nameof(request.Port));
            }

            if (string.IsNullOrWhiteSpace(request.Username))
            {
                throw new ArgumentException(ValidationMessages.RequiredUsername, nameof(request.Username));
            }

            if (isCreate && string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ArgumentException(ValidationMessages.RequiredPassword, nameof(request.Password));
            }

            if (string.IsNullOrWhiteSpace(request.FromEmail))
            {
                throw new ArgumentException(ValidationMessages.RequiredEmail, nameof(request.FromEmail));
            }

            if (string.IsNullOrWhiteSpace(request.FromName))
            {
                throw new ArgumentException(ValidationMessages.RequiredFromName, nameof(request.FromName));
            }
        }

        private static void ValidateType(SmtpSettingType type)
        {
            if (!Enum.IsDefined(type))
            {
                throw new ArgumentException(ValidationMessages.InvalidSmtpType, nameof(type));
            }

            if (type == SmtpSettingType.Unknown)
            {
                throw new ArgumentException(ValidationMessages.RequiredSmtpType, nameof(type));
            }
        }

        private static void EnsureSmtpSettingIsActiveForSending(SmtpSetting smtpSetting, SmtpSettingType type)
        {
            if (!smtpSetting.IsActive)
            {
                throw new DomainException(
                    ErrorCode.BusinessRuleViolation,
                    ErrorMessages.SmtpSettingInactive,
                    data: new
                    {
                        Type = type
                    });
            }
        }

        private static GetSmtpSettingRes ToResponse(SmtpSetting setting)
        {
            return new GetSmtpSettingRes
            {
                Id = setting.Id,
                Type = setting.Type,
                Host = setting.Host,
                Port = setting.Port,
                Username = setting.Username,
                HasPassword = !string.IsNullOrWhiteSpace(setting.Password),
                EnableSSL = setting.EnableSSL,
                FromEmail = setting.FromEmail,
                FromName = setting.FromName,
                IsActive = setting.IsActive,
                UpdatedAt = setting.UpdatedAt
            };
        }
    }
}
