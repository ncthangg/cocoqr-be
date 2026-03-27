using CocoQR.QR_Generator.Interface;
using CocoQR.QR_Generator.Models;
using QRCoder;

namespace CocoQR.QR_Generator.Image
{
    /// <summary>
    /// Render QR image dùng thư viện QRCoder (MIT license, không liên quan payment API).
    /// QRCoder chỉ convert chuỗi text → QR matrix → PNG — không gọi API nào.
    ///
    /// NuGet: QRCoder (https://github.com/codebude/QRCoder)
    /// </summary>
    public class QrImageRenderer : IQrImageRenderer
    {
        public QrImageResult Render(QrImageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Payload))
                throw new ArgumentException("Payload không được rỗng.");

            // EMVCo spec yêu cầu error correction level M
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(request.Payload, QRCodeGenerator.ECCLevel.M);
            using var qrCode = new PngByteQRCode(qrData);

            var pngBytes = qrCode.GetGraphic(
                pixelsPerModule: Math.Max(1, request.SizePixels / (qrData.ModuleMatrix.Count + 2 * request.Margin)),
                darkColorRgba: new byte[] { 0, 0, 0, 255 },       // đen
                lightColorRgba: new byte[] { 255, 255, 255, 255 }   // trắng
            );

            return new QrImageResult { PngBytes = pngBytes };
        }
    }
}
