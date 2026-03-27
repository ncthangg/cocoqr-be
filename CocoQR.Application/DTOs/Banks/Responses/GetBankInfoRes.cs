using CocoQR.Application.DTOs.Base.BaseRes;

namespace CocoQR.Application.DTOs.Banks.Responses
{
    public class GetBankInfoRes : BaseGetVM<Guid>
    {
        public string BankCode { get; set; } = string.Empty;
        public string? NapasBin { get; set; }
        public string? SwiftCode { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; }
    }
}
