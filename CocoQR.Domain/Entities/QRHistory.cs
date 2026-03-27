using CocoQR.Domain.Constants.Enum;

namespace CocoQR.Domain.Entities
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
        public string? BankShortNameSnapshot { get; set; }
        public string? NapasBinSnapshot { get; set; }
        // QR info
        public decimal? Amount { get; set; }
        public Currency Currency { get; set; } = Currency.VND;
        public string? Description { get; set; }

        public string? QrData { get; set; }
        public string TransactionRef { get; set; } = null!;

        public Guid ProviderId { get; set; }
        public QRReceiverType ReceiverType { get; set; }

        public bool IsFixedAmount { get; set; }
        public QrMode QrMode { get; set; }
        public QRStatus Status { get; set; } = QRStatus.CREATED;

        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? DeletedAt { get; set; }

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
            DeletedAt = DateTime.UtcNow;
        }
    }
}
