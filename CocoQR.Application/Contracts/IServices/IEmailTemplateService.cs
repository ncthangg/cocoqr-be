using CocoQR.Application.DTOs.Settings;

namespace CocoQR.Application.Contracts.IServices
{
    public interface IEmailTemplateService
    {
        Task<IReadOnlyList<GetEmailTemplateRes>> GetAllAsync(
            CancellationToken cancellationToken = default);
        Task<(string Subject, string Body)> RenderAsync(string templateKey, IReadOnlyDictionary<string, string>? variables = null);
    }
}
