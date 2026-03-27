namespace CocoQR.Application.DTOs.Banks.Requests
{
    public class PostBankInfoJsonReq
    {
        public string BankCode { get; set; } = string.Empty;
        public string? NapasBin { get; set; }
        public string? SwiftCode { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
