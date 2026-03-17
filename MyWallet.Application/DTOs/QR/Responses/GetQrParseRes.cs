namespace MyWallet.Application.DTOs.QR.Responses
{
    public class GetQrParseRes
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public string? BankBin { get; set; }
        public string? AccountNumber { get; set; }
        public decimal? Amount { get; set; }
        public string? Description { get; set; }
        public string? Currency { get; set; }
        public string? CountryCode { get; set; }
        public string? ServiceCode { get; set; }
        public string? MerchantField { get; set; }
        public string? PointOfInit { get; set; }
        public CrcInfo Crc { get; set; } = default!;

        public class CrcInfo
        {
            public string? Raw { get; set; }
            public string? Computed { get; set; }
            public bool IsMatch { get; set; }
        }
    }
}
