using MyWallet.Domain.Constants.Enum;

namespace MyWallet.Domain.Entities
{
    public class Account : BaseEntity
    {
        public Account() { }
        public Guid UserId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string? AccountHolder { get; set; }
        public string? BankCode { get; set; }
        public string? BankName { get; set; }
        public AccountProvider Provider { get; set; }
        public decimal? Balance { get; set; }
        public bool IsPinned { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public virtual User? User { get; set; }
        public virtual ICollection<QRHistory>? QRHistories { get; set; }

        public bool IsValidAccount()
        {
            return !string.IsNullOrWhiteSpace(AccountNumber)
                && Enum.IsDefined(typeof(AccountProvider), Provider);
        }
        public void Activate()
        {
            IsActive = true;
        }
        public void Deactivate()
        {
            IsActive = false;
        }
        public void Delete(Guid userId)
        {
            IsDeleted = true;
            SetDeleted(userId);
        }
    }
}
