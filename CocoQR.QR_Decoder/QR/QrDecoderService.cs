
using CocoQR.QR_Decoder.QR.Dto;
using CocoQR.QR_Decoder.QR.Interface;
using System.Drawing;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

namespace CocoQR.QR_Decoder.QR
{
    /// <summary>
    /// vì BarcodeReaderGeneric.Decode() không nhận Bitmap, mà cần LuminanceSource.
    /// Cần convert Bitmap → BitmapLuminanceSource.
    /// dotnet add package ZXing.Net.Bindings.Windows.Compatibility
    /// </summary>
    public class QrDecoderService : IQrDecoderService
    {
        public async Task<string> DecodeAsync(Stream imageStream)
        {
            using var bitmap = new Bitmap(imageStream);

            var source = new BitmapLuminanceSource(bitmap);

            ///Giới hạn chỉ đọc QR Code để tăng tốc:
            var reader = new BarcodeReaderGeneric
            {
                Options = new DecodingOptions
                {
                    PossibleFormats = new[] { BarcodeFormat.QR_CODE }
                }
            };

            var result = reader.Decode(source);

            return result?.Text;
        }

        public VietQrInfo ParsePayload(string payload)
        {
            return VietQrParser.Parse(payload);
        }
    }
}
