using CocoQR.QR_Generator.Models;

namespace CocoQR.QR_Generator.Interface
{
    /// <summary>
    /// Contract cho mỗi provider builder.
    /// Mỗi builder chịu trách nhiệm build đúng merchant info field (26/27/38)
    /// theo chuẩn của provider đó.
    ///
    /// Strategy pattern — QrPayloadEngine chọn builder phù hợp tại runtime.
    /// </summary>
    public interface IQrPayloadBuilder
    {
        /// <summary>
        /// Key dùng để lookup builder. Format: "{ProviderCode}_{Mode}".
        /// Ví dụ: "BANK_VietQR", "MOMO_VietQR", "MOMO_MomoNative"
        /// </summary>
        string BuilderKey { get; }

        /// <summary>
        /// Build merchant account info TLV string.
        /// Kết quả sẽ được nhúng vào root payload bởi QrPayloadEngine.
        /// </summary>
        /// <param name="request">Request đã được validate</param>
        /// <returns>Encoded TLV string của merchant field (ví dụ: "382C...")</returns>
        string BuildMerchantInfo(QrGenerateRequest request);

        /// <summary>Validate request trước khi build — throw nếu invalid</summary>
        void Validate(QrGenerateRequest request);
    }

}
