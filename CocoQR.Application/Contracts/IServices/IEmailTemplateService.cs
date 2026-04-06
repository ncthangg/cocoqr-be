using CocoQR.Application.DTOs.Settings;

namespace CocoQR.Application.Contracts.IServices
{
    public interface IEmailTemplateService
    {
        Task<IEnumerable<GetEmailTemplateRes>> GetAllAsync();
        Task<GetEmailTemplateRes> GetByIdAsync(Guid id);
        //Task<GetEmailTemplateRes> GetByKeyAsync(string templateKey);
        Task<Guid> PostAsync(PostEmailTemplateReq request);
        Task PutAsync(Guid id, PutEmailTemplateReq request);
        Task DeleteAsync(Guid id);
        Task<(string Subject, string Body)> RenderAsync(string templateKey, IReadOnlyDictionary<string, string>? variables = null);
    }
}
