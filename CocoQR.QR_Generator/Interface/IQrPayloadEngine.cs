using CocoQR.QR_Generator.Models;

namespace CocoQR.QR_Generator.Interface
{
    /// <summary>
    /// Core engine — orchestrate toàn bộ quá trình build payload.
    /// </summary>
    public interface IQrPayloadEngine
    {
        /// <summary>
        /// Generate EMVCo payload từ request.
        /// Throw <see cref="ArgumentException"/> nếu request invalid.
        /// Throw <see cref="NotSupportedException"/> nếu chưa có builder cho provider/mode.
        /// </summary>
        QrGenerateResult Generate(QrGenerateRequest request);

        /// <summary>Verify CRC của payload — dùng để validate QR đọc từ scan</summary>
        bool Verify(string payload);
    }

}
