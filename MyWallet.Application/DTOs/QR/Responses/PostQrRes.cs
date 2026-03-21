namespace MyWallet.Application.DTOs.QR.Responses
{
    public class PostQrRes
    {
        public long Id { get; set; }
        public string QrData { get; set; } = default!; // raw EMVCo payload
        public string QrImageUrl { get; set; } = default!; // "data:image/png;base64,..."
        public string TransactionRef { get; set; } = default!;
        public bool IsValid { get; set; }
    }
}
