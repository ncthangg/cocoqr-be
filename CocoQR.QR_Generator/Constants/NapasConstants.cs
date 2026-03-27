namespace CocoQR.QR_Generator.Constants
{
    /// <summary>
    /// Service codes theo chuẩn NAPAS/EMVCo.
    /// </summary>
    public static class NapasServiceCodes
    {
        /// <summary>Chuyển khoản theo số tài khoản — dùng phổ biến nhất</summary>
        public const string AccountTransfer = "QRIBFTTA";

        /// <summary>Chuyển khoản theo số thẻ</summary>
        public const string CardTransfer = "QRIBFTTC";
    }

    /// <summary>
    /// NAPAS AID (Application Identifier) — GUID cố định cho NAPAS scheme.
    /// </summary>
    public static class NapasRid
    {
        public const string Value = "A000000727";
    }
    public static class VnPayRid
    {
        public const string Value = "A000000775";
    }

    /// <summary>
    /// Giá trị cố định trong EMVCo payload.
    /// </summary>
    public static class EMVDefaults
    {
        public const string PayloadFormatIndicator = "01";
        public const string PointOfInitiationStatic = "11"; // QR tĩnh (reusable)
        public const string PointOfInitiationDynamic = "12"; // QR động (one-time)
        public const string CurrencyVND = "704";
        public const string CountryVN = "VN";
    }
}
