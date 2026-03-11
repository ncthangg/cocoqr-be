using MyWallet.Domain.Constants.Enum;

namespace MyWallet.Domain.Entities
{
    public class QRHistory
    {
        public QRHistory() { }
        public long Id { get; set; }
        public Guid? UserId { get; set; }
        public Guid? AccountId { get; set; }
        // Account info
        public string? AccountNumberSnapshot { get; set; }
        public string? AccountHolderSnapshot { get; set; }
        public string? BankCodeSnapshot { get; set; }
        public string? BankNameSnapshot { get; set; }
        // QR info
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public string? QRData { get; set; }
        public string? QRImageUrl { get; set; }

        public AccountProvider Provider { get; set; }
        public QRReceiverType ReceiverType { get; set; }

        public bool IsFixedAmount { get; set; }
        public bool IsPaid { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        
        public bool IsDeleted { get; set; }

        public virtual User? User { get; set; }
        public virtual Account? Account { get; set; }
        public bool IsValidQRHistory()
        {
            return UserId.HasValue
                   && AccountId.HasValue
                   && (IsFixedAmount ? Amount >= 0 : true);
        }
        public void Delete()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }
    }
}
