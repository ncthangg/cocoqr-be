using CocoQR.Application.DTOs.Base.BaseReq;

namespace CocoQR.Application.DTOs.Banks.Requests
{
    public class GetBankInfoReq : BaseReq
    {
        public bool? IsActive { get; set; }
        public string? SearchValue { get; set; }
    }
}