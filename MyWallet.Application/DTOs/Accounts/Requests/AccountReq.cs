using MyWallet.Application.DTOs.Base.BaseReq;
using MyWallet.Domain.Constants.Enum;

namespace MyWallet.Application.DTOs.Accounts.Requests
{
    public class GetAccountReq : BaseReq
    {
        public AccountProvider? Provider { get; set; }
        public string? SearchValue { get; set; }
        public bool? IsActive { get; set; }
    }
    public class GetAccountByAdminReq : BaseAdminReq
    {
        public Guid? UserId { get; set; }
        public AccountProvider? Provider { get; set; }
        public string? SearchValue { get; set; }
        public bool? IsActive { get; set; }
    }
    public class PostAccountReq
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string? AccountHolder { get; set; }
        public string? BankCode { get; set; }
        public string? BankName { get; set; }
        public AccountProvider Provider { get; set; }
        public bool IsActive { get; set; }
    }
    public class PutAccountReq
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string? AccountHolder { get; set; }
        public string? BankCode { get; set; } 
        public string? BankName { get; set; }
        public AccountProvider Provider { get; set; }
        public bool IsPinned { get; set; }
        public bool IsActive { get; set; }
    }
}
