namespace CocoQR.Application.DTOs.QR.Responses
{
    public class PostQrRes
    {
        public long Id { get; set; }
        public string QrData { get; set; } = default!; // raw EMVCo payload
        public string? StyleJson { get; set; }
        public string TransactionRef { get; set; } = default!;
        public bool IsValid { get; set; }
    }
}
