using CocoQR.Application.Contracts.IContext;
using CocoQR.Application.Contracts.IServices;
using CocoQR.Application.Contracts.ISubServices;
using CocoQR.Application.DTOs.Settings;
using CocoQR.Domain.Constants;
using ApplicationException = CocoQR.Application.Exceptions.ApplicationException;

namespace CocoQR.Application.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly ICocoMailClient _cocoMailClient;
        private readonly IUserContext _userContext;

        public EmailTemplateService(ICocoMailClient cocoMailClient, IUserContext userContext)
        {
            _cocoMailClient = cocoMailClient;
            _userContext = userContext;
        }

        public async Task<IReadOnlyList<GetEmailTemplateRes>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            EnsureAdmin();
            return (await _cocoMailClient.GetTemplatesAsync(cancellationToken))
                .OrderBy(x => x.TemplateKey)
                .Select(ToResponse)
                .ToList();
        }

        public async Task<(string Subject, string Body)> RenderAsync(
            string templateKey,
            IReadOnlyDictionary<string, string>? variables = null)
        {
            if (string.IsNullOrWhiteSpace(templateKey))
            {
                throw new ArgumentException("Template key is required.", nameof(templateKey));
            }

            var template = (await _cocoMailClient.GetTemplatesAsync())
                .FirstOrDefault(x =>
                    x.IsActive &&
                    string.Equals(x.TemplateKey, templateKey.Trim(), StringComparison.OrdinalIgnoreCase))
                ?? throw new ApplicationException(
                    ErrorCode.NotFound,
                    $"Template '{templateKey}' was not found.");

            return (
                RenderTemplate(template.Subject, variables),
                RenderTemplate(template.Html, variables));
        }

        private void EnsureAdmin()
        {
            if (!_userContext.IsAdmin())
            {
                throw new ApplicationException(ErrorCode.Unauthorized, ErrorMessages.Unauthorized);
            }
        }

        private static GetEmailTemplateRes ToResponse(CocoMailTemplateResponse template)
        {
            return new GetEmailTemplateRes
            {
                Id = template.Id,
                TemplateKey = template.TemplateKey,
                Subject = template.Subject,
                Html = template.Html,
                Version = template.Version,
                IsActive = template.IsActive,
                Placeholders = template.Placeholders ?? []
            };
        }

        private static string RenderTemplate(
            string value,
            IReadOnlyDictionary<string, string>? variables)
        {
            if (string.IsNullOrEmpty(value) || variables == null || variables.Count == 0)
            {
                return value;
            }

            var rendered = value;
            foreach (var pair in variables)
            {
                rendered = rendered.Replace(
                    $"{{{{{pair.Key}}}}}",
                    pair.Value ?? string.Empty,
                    StringComparison.OrdinalIgnoreCase);
            }

            return rendered;
        }
    }
}
