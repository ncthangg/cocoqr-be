using CocoQR.Application.DTOs.Base.BaseRes;
using CocoQR.Domain.Constants.Enum;

namespace CocoQR.Application.DTOs.Accounts.Responses
{
    public class GetAccountRes : BaseGetVM<Guid>
    {
        public Guid UserId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string? AccountHolder { get; set; }

        public string? BankCode { get; set; }
        public string? NapasBin { get; set; }
        public string? BankName { get; set; }
        public string? BankShortName { get; set; }
        public string? BankLogoUrl { get; set; }
        public bool? BankIsActive { get; set; }
        public bool? BankStatus { get; set; }

        public Guid ProviderId { get; set; }
        public ProviderCode ProviderCode { get; set; }
        public string? ProviderName { get; set; }
        public string? ProviderLogoUrl { get; set; }
        public bool? ProviderIsActive { get; set; }
        public bool? ProviderStatus { get; set; }


        public decimal? Balance { get; set; }
        public bool IsPinned { get; set; }
        public bool IsActive { get; set; }
    }
}
