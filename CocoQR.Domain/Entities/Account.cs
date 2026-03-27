namespace CocoQR.Domain.Entities
{
    public class Account : BaseEntity
    {
        public Account() { }
        public Guid UserId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string? AccountHolder { get; set; }
        public string? BankCode { get; set; }
        public Guid ProviderId { get; set; }
        public decimal? Balance { get; set; }
        public bool IsPinned { get; set; }

        public bool IsActive { get; set; }
        public virtual User? User { get; set; }
        public virtual ICollection<QRHistory>? QRHistories { get; set; }

        public bool IsValidAccount()
        {
            return !string.IsNullOrWhiteSpace(AccountNumber);
        }
        public void Activate()
        {
            IsActive = true;
        }
        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
