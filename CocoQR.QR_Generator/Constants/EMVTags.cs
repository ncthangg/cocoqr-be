namespace CocoQR.QR_Generator.Constants
{
    /// <summary>
    /// EMVCo QR Payment tag IDs — theo spec v1.1
    /// Không thay đổi giá trị này trừ khi spec thay đổi.
    /// </summary>
    public static class EMVTags
    {
        // ── Root fields ──────────────────────────────────────────────────────────
        public const string PayloadFormatIndicator = "00"; // luôn = "01"
        public const string PointOfInitiation = "01"; // "11" static | "12" dynamic
        public const string TransactionCurrency = "53"; // "704" = VND
        public const string TransactionAmount = "54"; // optional
        public const string CountryCode = "58"; // "VN"
        public const string AdditionalData = "62";
        public const string CRC = "63"; // phải là field cuối cùng

        // ── Merchant account info fields (top-level) ─────────────────────────────
        /// <summary>Field 26 — MOMO interbank (VietQR profile)</summary>
        public const string MerchantVietQR = "26";
        /// <summary>Field 27 — MOMO native (SĐT, chỉ app MoMo đọc)</summary>
        public const string MerchantMomoNative = "27";
        /// <summary>Field 38 — NAPAS bank transfer</summary>
        public const string MerchantNapas = "38";

        // ── Sub-tags bên trong Merchant account info ─────────────────────────────
        public const string MerchantGuid = "00"; // "A000000727"
        public const string MerchantAccountBlock = "01"; // "QRIBFTTA" | "QRIBFTTC"
        public const string MerchantServiceCode = "02";

        // ── Sub-tags bên trong AccountInfo (nested trong MerchantAccountInfo) ────
        public const string AccountBin = "00"; // NapasBin 6 chữ số
        public const string AccountNumber = "01"; // số tài khoản / SĐT

        // ── Sub-tags bên trong AdditionalData ────────────────────────────────────
        public const string BillNumber = "01";
        public const string MobileNumber = "02";
        public const string StoreLabel = "03";
        public const string LoyaltyNumber = "04";
        public const string ReferenceLabel = "05";
        public const string CustomerLabel = "06";
        public const string TerminalLabel = "07";
        public const string AdditionalPurpose = "08"; // nội dung chuyển tiền
        public const string ConsumerDataRequest = "09";
    }
}
