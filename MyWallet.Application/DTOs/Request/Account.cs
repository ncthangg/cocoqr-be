namespace MyWallet.Application.DTOs.Request
{
    public class PostAccountReq
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountHolder { get; set; } = string.Empty;
        public string BankCode { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string? AccountType { get; set; }
    }
    public class PutAccountReq
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountHolder { get; set; } = string.Empty;
        public string BankCode { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string? AccountType { get; set; }
    }
}
