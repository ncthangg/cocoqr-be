using CocoQR.Application.DTOs.Base.BaseReq;

namespace CocoQR.Application.DTOs.QR.Requests
{
    public class GetQrReq : BaseReq
    {
        public Guid? ProviderId { get; set; }
        public string? SearchValue { get; set; }
    }
    public class GetQrByAdminReq : BaseAdminReq
    {
        public Guid? UserId { get; set; }
        public Guid? ProviderId { get; set; }
        public string? SearchValue { get; set; }
    }
}
