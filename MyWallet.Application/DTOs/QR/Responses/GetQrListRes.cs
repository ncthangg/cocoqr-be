namespace MyWallet.Application.DTOs.QR.Responses
{
    public class GetQrListRes
    {
        public Guid Id { get; set; }
        public string TransactionRef { get; set; } = default!;
        public string? BankCode { get; set; }
        public string? AccountNumber { get; set; }
        public decimal? Amount { get; set; }
        public string? Description { get; set; }
        public bool IsFixedAmount { get; set; }
        public DateTime CreatedAt { get; set; }

        // QrImageUrl KHÔNG có trong list response — generate on-demand qua /qr/{id}/image
    }
}
