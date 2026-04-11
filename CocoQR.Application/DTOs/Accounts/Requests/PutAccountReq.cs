namespace CocoQR.Application.DTOs.Accounts.Requests
{
    public class PutAccountReq
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string? AccountHolder { get; set; }
        public string? BankCode { get; set; }
        public Guid ProviderId { get; set; }
        public bool IsActive { get; set; }
    }
    public class PatchAccountRequest
    {
        public bool isPinned { get; set; }
    }
}
