using CocoQR.QR_Generator.Constants;
using CocoQR.QR_Generator.Encoders;
using CocoQR.QR_Generator.Interface;
using CocoQR.QR_Generator.Models;

namespace CocoQR.QR_Generator.Builders;

/// <summary>
/// Builder cho chuyển khoản ngân hàng qua NAPAS — Field 38.
///
/// Cấu trúc payload được build:
/// <code>
/// 38 {len}
///   00 10 A000000727          ← NAPAS AID
///   01 08 QRIBFTTA            ← service code (account transfer)
///   02 {len}
///     00 06 {NapasBin}        ← BIN ngân hàng (6 chữ số)
///     01 {len} {AccountNumber} ← số tài khoản
/// </code>
///
/// Quét được bởi: mọi app ngân hàng, MoMo, ZaloPay, camera iOS/Android.
/// </summary>
public class NapasQrBuilder : IQrPayloadBuilder
{
    public string BuilderKey => BuilderKeys.Bank;

    public void Validate(QrGenerateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.BankCode))
            throw new ArgumentException("BankCode bắt buộc với NAPAS builder.");

        if (string.IsNullOrWhiteSpace(request.AccountNumber))
            throw new ArgumentException("AccountNumber bắt buộc với NAPAS builder.");

        var napasBin = request.NapasBinOverride ?? (BankBins.Exists(request.BankCode)
                ? BankBins.GetBin(request.BankCode)
                : throw new ArgumentException($"BankCode '{request.BankCode}' không tìm thấy trong BankBins."));

        if (napasBin.Length != 6 || !napasBin.All(char.IsDigit))
            throw new ArgumentException($"NapasBin phải đúng 6 chữ số, nhận: '{napasBin}'");
    }

    public string BuildMerchantInfo(QrGenerateRequest request)
    {
        var napasBin = request.NapasBinOverride ?? BankBins.GetBin(request.BankCode);

        // Inner nested: BIN + AccountNumber
        var accountBlock = TLVEncoder.EncodeMany(
            (EMVTags.AccountBin, napasBin),                              // 00
            (EMVTags.AccountNumber, request.AccountNumber)                  // 01
        );

        // Merchant block: GUID + ServiceCode + AccountInfo
        var merchantBlock = TLVEncoder.EncodeMany(
            (EMVTags.MerchantGuid, NapasRid.Value),
            (EMVTags.MerchantAccountBlock, accountBlock),
            (EMVTags.MerchantServiceCode, NapasServiceCodes.AccountTransfer)
        );

        // Field 38 = NAPAS bank transfer
        return TLVEncoder.Encode(EMVTags.MerchantNapas, merchantBlock);
    }
}