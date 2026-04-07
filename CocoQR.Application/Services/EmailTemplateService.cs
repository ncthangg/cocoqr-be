using CocoQR.Application.Contracts.ICache;
using CocoQR.Application.Contracts.IContext;
using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Application.Contracts.IUnitOfWork;
using CocoQR.Application.DTOs.Settings;
using CocoQR.Domain.Constants;
using CocoQR.Domain.Entities;
using ApplicationException = CocoQR.Application.Exceptions.ApplicationException;
using DomainException = CocoQR.Domain.Exceptions.DomainException;

namespace CocoQR.Application.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private static readonly TimeSpan EmailTemplatesCacheExpiry = TimeSpan.FromMinutes(5);
        private const string EmailTemplatesCacheKey = "emailTemplate:system";
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly IIdGenerator _idGenerator;
        private readonly ICacheService _cacheService;

        public EmailTemplateService(IUnitOfWork unitOfWork, IUserContext userContext, IIdGenerator idGenerator, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _userContext = userContext;
            _idGenerator = idGenerator;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<GetEmailTemplateRes>> GetAllAsync()
        {
            EnsureAdmin();

            var cached = await _cacheService
                .GetAsync<IEnumerable<GetEmailTemplateRes>>(EmailTemplatesCacheKey);

            if (cached != null)
                return cached;

            var templates = await _unitOfWork.EmailTemplates.GetAllAsync();

            var result = templates
                .OrderBy(x => x.TemplateKey)
                .Select(ToResponse)
                .ToList();

            await _cacheService.SetAsync(
                EmailTemplatesCacheKey,
                result,
                EmailTemplatesCacheExpiry
            );

            return result;
        }
        public async Task<GetEmailTemplateRes> GetByIdAsync(Guid id)
        {
            EnsureAdmin();

            if (id == Guid.Empty)
            {
                throw new ArgumentException("Id template không hợp lệ.", nameof(id));
            }

            var template = await _unitOfWork.EmailTemplates.GetByIdAsync(id)
                ?? throw new ApplicationException(ErrorCode.NotFound, "Không tìm thấy template.");

            return ToResponse(template);
        }

        //public async Task<GetEmailTemplateRes> GetByKeyAsync(string templateKey)
        //{
        //    if (string.IsNullOrWhiteSpace(templateKey))
        //    {
        //        throw new ArgumentException("Template key là bắt buộc.", nameof(templateKey));
        //    }

        //    templateKey = templateKey.Trim();

        //    var cacheKey = $"emailTemplates:key:{templateKey}";

        //    var cached = await _cacheService.GetAsync<GetEmailTemplateRes>(cacheKey);

        //    if (cached != null)
        //        return cached;

        //    var template = await _unitOfWork.EmailTemplates.GetByKeyAsync(templateKey);

        //    if (template == null)
        //    {
        //        throw new ApplicationException(
        //            ErrorCode.NotFound,
        //            $"Không tìm thấy template '{templateKey}'.");
        //    }

        //    var result = ToResponse(template);

        //    await _cacheService.SetAsync(
        //        cacheKey,
        //        result,
        //        TimeSpan.FromHours(1)
        //    );

        //    return result;
        //}

        public async Task<Guid> PostAsync(PostEmailTemplateReq request)
        {
            ArgumentNullException.ThrowIfNull(request);

            EnsureAdmin();
            ValidateRequest(request.TemplateKey, request.Subject, request.Body);

            var normalizedKey = request.TemplateKey.Trim();
            var duplicated = await _unitOfWork.EmailTemplates.GetByKeyAsync(normalizedKey, onlyActive: false);

            if (duplicated != null)
            {
                throw new DomainException(ErrorCode.DuplicateEntry, "Template key đã tồn tại.");
            }

            var entity = new EmailTemplate
            {
                Id = _idGenerator.NewId(),
                TemplateKey = normalizedKey,
                Subject = request.Subject.Trim(),
                Body = request.Body,
                IsActive = request.IsActive,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.EmailTemplates.AddAsync(entity);

            await _cacheService.RemoveAsync(EmailTemplatesCacheKey);
            return entity.Id;
        }

        public async Task PutAsync(Guid id, PutEmailTemplateReq request)
        {
            ArgumentNullException.ThrowIfNull(request);

            EnsureAdmin();

            if (id == Guid.Empty)
            {
                throw new ArgumentException("Id template không hợp lệ.", nameof(id));
            }

            ValidateRequest(request.TemplateKey, request.Subject, request.Body);

            var entity = await _unitOfWork.EmailTemplates.GetByIdAsync(id)
                ?? throw new ApplicationException(ErrorCode.NotFound, "Không tìm thấy template.");

            var normalizedKey = request.TemplateKey.Trim();
            if (!string.Equals(entity.TemplateKey, normalizedKey, StringComparison.OrdinalIgnoreCase))
            {
                var duplicated = await _unitOfWork.EmailTemplates.GetByKeyAsync(normalizedKey, onlyActive: false);
                if (duplicated != null && duplicated.Id != id)
                {
                    throw new DomainException(ErrorCode.DuplicateEntry, "Template key đã tồn tại.");
                }
            }

            entity.TemplateKey = normalizedKey;
            entity.Subject = request.Subject.Trim();
            entity.Body = request.Body;
            entity.IsActive = request.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.EmailTemplates.UpdateAsync(entity);
            await _cacheService.RemoveAsync(EmailTemplatesCacheKey);
        }

        public async Task DeleteAsync(Guid id)
        {
            EnsureAdmin();

            if (id == Guid.Empty)
            {
                throw new ArgumentException("Id template không hợp lệ.", nameof(id));
            }

            var exists = await _unitOfWork.EmailTemplates.GetByIdAsync(id);
            if (exists == null)
            {
                throw new ApplicationException(ErrorCode.NotFound, "Không tìm thấy template.");
            }

            await _unitOfWork.EmailTemplates.DeleteAsync(id);
            await _cacheService.RemoveAsync(EmailTemplatesCacheKey);
        }

        public async Task<(string Subject, string Body)> RenderAsync(string templateKey, IReadOnlyDictionary<string, string>? variables = null)
        {
            if (string.IsNullOrWhiteSpace(templateKey))
            {
                throw new ArgumentException("Template key là bắt buộc.", nameof(templateKey));
            }

            var template = await _unitOfWork.EmailTemplates.GetByKeyAsync(templateKey.Trim(), onlyActive: true)
                ?? throw new ApplicationException(ErrorCode.NotFound, $"Không tìm thấy template '{templateKey}'.");

            var subject = RenderTemplate(template.Subject, variables);
            var body = RenderTemplate(template.Body, variables);

            return (subject, body);
        }

        private void EnsureAdmin()
        {
            if (!_userContext.IsAdmin())
            {
                throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);
            }
        }

        private static void ValidateRequest(string templateKey, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(templateKey))
            {
                throw new ArgumentException("Template key là bắt buộc.", nameof(templateKey));
            }

            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentException(ValidationMessages.RequiredSubject, nameof(subject));
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                throw new ArgumentException(ValidationMessages.RequiredBody, nameof(body));
            }
        }

        private static GetEmailTemplateRes ToResponse(EmailTemplate template)
        {
            return new GetEmailTemplateRes
            {
                Id = template.Id,
                TemplateKey = template.TemplateKey,
                Subject = template.Subject,
                Body = template.Body,
                IsActive = template.IsActive,
                UpdatedAt = template.UpdatedAt
            };
        }

        private static string RenderTemplate(string value, IReadOnlyDictionary<string, string>? variables)
        {
            if (string.IsNullOrEmpty(value) || variables == null || variables.Count == 0)
            {
                return value;
            }

            var rendered = value;
            foreach (var pair in variables)
            {
                rendered = rendered.Replace($"{{{{{pair.Key}}}}}", pair.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            }

            return rendered;
        }
    }
}
