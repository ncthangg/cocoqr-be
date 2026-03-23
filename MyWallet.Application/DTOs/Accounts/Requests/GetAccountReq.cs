using MyWallet.Application.DTOs.Base.BaseReq;

namespace MyWallet.Application.DTOs.Accounts.Requests
{
    public class GetAccountReq : BaseReq
    {
        public Guid? ProviderId { get; set; }
        public string? SearchValue { get; set; }
        public bool? IsActive { get; set; }
    }
    public class GetAccountByAdminReq : BaseAdminReq
    {
        public Guid? UserId { get; set; }
        public Guid? ProviderId { get; set; }
        public string? SearchValue { get; set; }
        public bool? IsActive { get; set; }
    }

}
