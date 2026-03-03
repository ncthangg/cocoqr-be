namespace MyWallet.Domain.Entities
{
    public class Account : BaseEntity
    {
        public Guid UserId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountHolder { get; set; } = string.Empty;
        public string BankCode { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;

        public decimal? Balance { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual User? User { get; set; }
        public virtual ICollection<QRHistory>? QRHistories { get; set; }

        public bool IsValidAccount()
        {
            return !string.IsNullOrWhiteSpace(AccountNumber)
                && !string.IsNullOrWhiteSpace(AccountHolder)
                && !string.IsNullOrWhiteSpace(BankCode);
        }
    }
}
