namespace MyWallet.Domain.Entities
{
    public class QRHistory
    {
        public long Id { get; set; }
        public Guid? UserId { get; set; }
        public Guid? AccountId { get; set; }

        // QR info
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public string? QRData { get; set; }
        public string? QRImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        // Navigation properties
        public virtual User? User { get; set; }
        public virtual Account? Account { get; set; }
        public bool IsValidQRHistory()
        {
            return UserId.HasValue
                   && AccountId.HasValue
                   && Amount > 0;
        }
    }
}
