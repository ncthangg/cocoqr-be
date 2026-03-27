namespace CocoQR.QR_Decoder.QR.Dto
{
    public class VietQrInfo
    {
        public string? RawPayload { get; set; }

        public string? NapasRid { get; set; }

        public string? BankBin { get; set; }

        public string? BankCode { get; set; }

        public string? BankName { get; set; }

        public string? AccountNumber { get; set; }

        public string? AccountName { get; set; }

        public string? Amount { get; set; }

        public string? Note { get; set; }

        public string? TransferType { get; set; }

        public bool CRCValid { get; set; }
    }

}
