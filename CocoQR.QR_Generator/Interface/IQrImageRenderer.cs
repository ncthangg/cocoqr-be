using CocoQR.QR_Generator.Models;

namespace CocoQR.QR_Generator.Interface
{
    /// <summary>
    /// Convert payload string → QR image PNG bytes.
    /// Tách khỏi payload engine vì sau này sẽ thêm customize (logo, màu, style).
    /// </summary>
    public interface IQrImageRenderer
    {
        /// <summary>
        /// Render QR image từ payload.
        /// </summary>
        /// <returns><see cref="QrImageResult"/> chứa PNG bytes và Base64</returns>
        QrImageResult Render(QrImageRequest request);
    }

}
