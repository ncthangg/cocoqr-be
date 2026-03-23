using MyWallet.Domain.Constants.Enum;
using System.ComponentModel.DataAnnotations;

namespace MyWallet.Application.DTOs.QR.Requests
{
    public class PostQrReq
    {
        public required Guid ProviderId { get; set; }

        public Guid? AccountId { get; set; }
        [MaxLength(50)]
        public string? AccountNumber { get; set; }
        [MaxLength(20)]
        public string? BankCode { get; set; }
        [Range(0, 999_000_000_000)]
        public decimal? Amount { get; set; }
        [MaxLength(99)]
        public string? Description { get; set; }

        public bool IsFixedAmount { get; set; }

        /// <summary>
        /// "VietQR" (default) hoặc "MomoNative" (chỉ app MoMo quét được).
        /// </summary>
        public QrMode? QrMode { get; set; } = Domain.Constants.Enum.QrMode.VietQR;
        public Guid? StyleId { get; set; }
        public bool UseDefault { get; set; }
        public string? StyleJson { get; set; }
    }
}
