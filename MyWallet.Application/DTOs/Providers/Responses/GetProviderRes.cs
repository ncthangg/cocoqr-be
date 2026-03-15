using MyWallet.Application.DTOs.Base.BaseRes;
using MyWallet.Domain.Constants.Enum;

namespace MyWallet.Application.DTOs.Providers.Responses
{ 
    public class GetProviderRes : BaseGetVM
    {
        public required ProviderCode Code { get; set; }
        public required string Name { get; set; }
        public bool IsActive { get; set; }
        public string? LogoUrl { get; set; }
    }
}
