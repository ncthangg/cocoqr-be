using CocoQR.QR_Generator.Constants;
using CocoQR.QR_Generator.Encoders;
using CocoQR.QR_Generator.Exceptions;
using CocoQR.QR_Generator.Interface;
using CocoQR.QR_Generator.Models;

namespace CocoQR.QR_Generator.Builders
{
    /// <summary>
    /// Builder cho VNPay — Field 26, NAPAS interbank routing.
    ///
    /// VNPay trong hệ thống NAPAS hoạt động như một bank bình thường.
    /// NapasBin của VNPay: 970436 (routing qua VCB gateway — verify lại với NAPAS).
    ///
    /// AccountNumber = merchant code hoặc virtual account do VNPay cấp.
    /// </summary>
    public class VnPayQrBuilder : IQrPayloadBuilder
    {
        public string BuilderKey => BuilderKeys.VnPay;

        public void Validate(QrGenerateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AccountNumber))
                throw new QrValidationException(nameof(request.AccountNumber),
                    "AccountNumber (VNPay merchant/virtual account) bắt buộc.");

            if (string.IsNullOrWhiteSpace(request.NapasBinOverride)
                && !BankBins.Exists("VNPAY"))
                throw new QrBankNotFoundException("VNPAY");
        }

        public string BuildMerchantInfo(QrGenerateRequest request)
        {
            var merchantBlock = TLVEncoder.EncodeMany(
                (EMVTags.MerchantGuid, VnPayRid.Value)
            //(EMVTags.MerchantId, request.MerchantId), //mã định danh trên hệ thống vnpay
            //(EMVTags.TerminalId, request.TerminalId), //điểm thanh toán (vị trí)
            //(EMVTags.TransactionRef, request.TransactionRef) //mã giao dịch (tạo mỗi lần thanh toán)
            );

            return TLVEncoder.Encode(EMVTags.MerchantVietQR, merchantBlock); // Field 26
        }
    }
}
