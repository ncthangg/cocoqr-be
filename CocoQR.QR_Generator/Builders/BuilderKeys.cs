using CocoQR.Domain.Constants.Enum;

namespace CocoQR.QR_Generator.Builders
{
    /// <summary>
    /// Lookup keys cho builder registry.
    /// Format: {ProviderCode}_{Mode} — dùng để tìm builder tương ứng.
    /// </summary>
    public static class BuilderKeys
    {
        public const string Bank = "BANK_VietQR";
        public const string MomoVietQr = "MOMO_VietQR";
        public const string VnPay = "VNPAY_VietQR";
        public const string ZaloPay = "ZALOPAY_VietQR";

        public const string MomoNative = "MOMO_MomoNative";

        /// <summary>
        /// Tạo key từ bankCode và mode.
        /// </summary>
        public static string From(string providerCode, QrMode mode)
        {
            var code = providerCode.ToUpperInvariant() switch
            {
                "MOMO" => "MOMO",
                "VNPAY" => "VNPAY",
                "ZALOPAY" => "ZALOPAY",
                _ => "BANK",
            };
            return $"{code}_{mode}";
        }
    }
}
