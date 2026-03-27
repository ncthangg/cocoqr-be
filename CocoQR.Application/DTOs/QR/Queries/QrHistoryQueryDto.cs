using CocoQR.Domain.Constants.Enum;

namespace CocoQR.Application.DTOs.QR.Queries
{
    public class QrHistoryQueryDto
    {
        public long Id { get; set; }
        public Guid? UserId { get; set; }
        public string? Email { get; set; }
        public Guid? AccountId { get; set; }
        // Account info
        public string? AccountNumberSnapshot { get; set; }
        public string? AccountHolderSnapshot { get; set; }

        public string? BankCodeSnapshot { get; set; }
        public string? NapasBinSnapshot { get; set; }
        public string? BankNameSnapshot { get; set; }
        public string? BankShortNameSnapshot { get; set; }

        // QR info
        public decimal? Amount { get; set; }
        public Currency? Currency { get; set; }
        public string? Description { get; set; }

        public string? QrData { get; set; }
        public string? TransactionRef { get; set; }

        public Guid? ProviderId { get; set; }
        public string? ProviderCode { get; set; }
        public string? ProviderName { get; set; }
        public string? ProviderLogoUrl { get; set; }

        public QRReceiverType? ReceiverType { get; set; }
        public bool? IsFixedAmount { get; set; }
        public QrMode? QrMode { get; set; }
        public QRStatus? QrStatus { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
