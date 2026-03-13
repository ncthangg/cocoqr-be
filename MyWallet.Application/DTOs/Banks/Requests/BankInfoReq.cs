using Microsoft.AspNetCore.Http;

namespace MyWallet.Application.DTOs.Banks.Requests
{
    public class PostBankInfoReq
    {
        public string BankCode { get; set; } = string.Empty;
        public string? NapasCode { get; set; }
        public string? SwiftCode { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public IFormFile? LogoUrl { get; set; }
        public bool IsActive { get; set; }
    }
    public class PutBankInfoReq
    {
        public string BankCode { get; set; } = string.Empty;
        public string? NapasCode { get; set; }
        public string? SwiftCode { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public IFormFile? LogoUrl { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleteFile { get; set; } = false;
    }
    public class PostBankInfoJsonReq
    {
        public string BankCode { get; set; } = string.Empty;
        public string? NapasCode { get; set; }
        public string? SwiftCode { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; }
    }
}
