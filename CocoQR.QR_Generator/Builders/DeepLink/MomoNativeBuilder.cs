using CocoQR.QR_Generator.Constants;
using CocoQR.QR_Generator.Encoders;
using CocoQR.QR_Generator.Interface;
using CocoQR.QR_Generator.Models;

namespace CocoQR.QR_Generator.Builders.DeepLink
{
    /// <summary>
    /// Builder cho MoMo native — Field 27.
    ///
    /// Cấu trúc:
    /// <code>
    /// 27 {len}
    ///   00 10 A000000727
    ///   01 08 QRIBFTTM             ← MoMo native service code
    ///   02 {len} {PhoneNumber}     ← SĐT đăng ký MoMo (không kèm BIN)
    /// </code>
    ///
    /// ⚠️ Chỉ quét được bởi app MoMo.
    ///    Camera iOS/Android và app ngân hàng KHÔNG hiểu field 27.
    ///    Dùng khi user muốn nhận tiền từ user MoMo khác bằng SĐT.
    /// </summary>
    public class MomoNativeBuilder : IQrPayloadBuilder
    {
        private const string MomoNativeServiceCode = "QRIBFTTM";

        public string BuilderKey => BuilderKeys.MomoNative;

        public void Validate(QrGenerateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AccountNumber))
                throw new ArgumentException("Số điện thoại MoMo (AccountNumber) bắt buộc với MoMo Native builder.");

            // Validate SĐT VN: 10 chữ số, bắt đầu bằng 0
            var phone = request.AccountNumber.Trim();
            if (!phone.All(char.IsDigit) || phone.Length != 10 || !phone.StartsWith('0'))
                throw new ArgumentException(
                    $"Số điện thoại MoMo không hợp lệ: '{phone}'. Phải là 10 chữ số bắt đầu bằng 0.");
        }

        public string BuildMerchantInfo(QrGenerateRequest request)
        {
            var merchantBlock = TLVEncoder.EncodeMany(
                (EMVTags.MerchantGuid, NapasRid.Value),
                (EMVTags.MerchantAccountBlock, MomoNativeServiceCode)
            // (EMVTags.MerchantAccountInfo, request.AccountNumber.Trim())
            );

            // Field 27 = MoMo native (SĐT)
            return TLVEncoder.Encode(EMVTags.MerchantMomoNative, merchantBlock);
        }
    }
}
