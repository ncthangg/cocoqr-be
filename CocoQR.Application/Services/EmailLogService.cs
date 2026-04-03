using CocoQR.Application.Contracts.IContext;
using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Application.Common.Mapper;
using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Application.DTOs.EmailLogs.Requests;
using CocoQR.Application.DTOs.EmailLogs.Responses;
using CocoQR.Domain.Constants;
using CocoQR.Domain.Constants.Enum;
using ApplicationException = CocoQR.Application.Exceptions.ApplicationException;

namespace CocoQR.Application.Services
{
    public class EmailLogService : IEmailLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;

        public EmailLogService(IUnitOfWork unitOfWork, IUserContext userContext)
        {
            _unitOfWork = unitOfWork;
            _userContext = userContext;
        }

        public async Task<PagingVM<GetEmailLogRes>> GetAsync(GetEmailLogReq request)
        {
            EnsureAdmin();
            ArgumentNullException.ThrowIfNull(request);

            if (request.Type.HasValue && !Enum.IsDefined(request.Type.Value))
            {
                throw new ArgumentException(ValidationMessages.InvalidSmtpType, nameof(request.Type));
            }

            if (request.Type == SmtpSettingType.Unknown)
            {
                throw new ArgumentException(ValidationMessages.RequiredSmtpType, nameof(request.Type));
            }

            if (request.Status.HasValue && !Enum.IsDefined(request.Status.Value))
            {
                throw new ArgumentException("Trạng thái email log không hợp lệ.", nameof(request.Status));
            }

            if (request.Direction.HasValue && !Enum.IsDefined(request.Direction.Value))
            {
                throw new ArgumentException("Chiều email không hợp lệ.", nameof(request.Direction));
            }

            var smtpTypeFilter = request.Type.HasValue
                ? request.Type
                : null;

            if (request.FromDate.HasValue && request.ToDate.HasValue && request.FromDate.Value.Date > request.ToDate.Value.Date)
            {
                throw new ArgumentException("FromDate không được lớn hơn ToDate.", nameof(request.FromDate));
            }

            var fromDate = request.FromDate?.Date;
            var toDateExclusive = request.ToDate?.Date.AddDays(1);
            var toEmail = string.IsNullOrWhiteSpace(request.ToEmail) ? null : request.ToEmail.Trim();
            var recipientFullName = string.IsNullOrWhiteSpace(request.RecipientFullName) ? null : request.RecipientFullName.Trim();
            var subject = string.IsNullOrWhiteSpace(request.Subject) ? null : request.Subject.Trim();

            var (items, totalCount) = await _unitOfWork.EmailLogs.GetAsync(
                request.PageNumber,
                request.PageSize,
                smtpTypeFilter,
                request.Status,
                request.Direction,
                request.RecipientUserId,
                toEmail,
                recipientFullName,
                subject,
                fromDate,
                toDateExclusive);

            return new PagingVM<GetEmailLogRes>
            {
                List = items.Select(EmailLogMapper.ToListResponse),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };
        }

        public async Task<GetEmailLogByIdRes> GetByIdAsync(Guid id)
        {
            EnsureAdmin();

            if (id == Guid.Empty)
            {
                throw new ArgumentException("Invalid ID", nameof(id));
            }

            var item = await _unitOfWork.EmailLogs.GetByIdDetailAsync(id)
                ?? throw new ApplicationException(ErrorCode.NotFound, ErrorMessages.EntityNotFound);

            return EmailLogMapper.ToDetailResponse(item);
        }

        private void EnsureAdmin()
        {
            if (!_userContext.IsAdmin())
            {
                throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);
            }
        }

    }
}