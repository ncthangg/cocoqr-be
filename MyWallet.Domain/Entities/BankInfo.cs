namespace MyWallet.Domain.Entities
{
    public class BankInfo : BaseEntity
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
