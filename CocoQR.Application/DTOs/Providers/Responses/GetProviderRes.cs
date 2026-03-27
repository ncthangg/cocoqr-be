using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Domain.Constants.Enum;

namespace CocoQR.Application.DTOs.Providers.Responses
{
    public class GetProviderRes : BaseGetVM<Guid>
    {
        public required ProviderCode Code { get; set; }
        public required string Name { get; set; }
        public bool IsActive { get; set; }
        public string? LogoUrl { get; set; }
    }
}
