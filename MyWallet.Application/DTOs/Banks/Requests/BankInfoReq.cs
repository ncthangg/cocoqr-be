using Microsoft.AspNetCore.Http;

namespace MyWallet.Application.DTOs.Banks.Requests
{
    public class PutBankInfoReq
    {
        public string BankCode { get; set; } = string.Empty;
        public IFormFile? LogoUrl { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleteFile { get; set; } = false;
    }
}
