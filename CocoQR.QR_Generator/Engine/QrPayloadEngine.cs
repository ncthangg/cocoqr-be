using CocoQR.QR_Generator.Builders;
using CocoQR.QR_Generator.Constants;
using CocoQR.QR_Generator.Encoders;
using CocoQR.QR_Generator.Exceptions;
using CocoQR.QR_Generator.Interface;
using CocoQR.QR_Generator.Models;
using System.Text;

namespace CocoQR.QR_Generator.Engine
{
    /// <summary>
    /// Orchestrator — build EMVCo QR payload theo chuẩn VietQR/NAPAS.
    ///
    /// Flow:
    ///   1. Lookup builder theo (BankCode, Mode)
    ///   2. Validate request
    ///   3. Build root fields (00, 01)
    ///   4. Gọi builder.BuildMerchantInfo() → field 26/27/38
    ///   5. Append currency, amount, country, description
    ///   6. Append CRC field header "6304", tính CRC, append CRC value
    ///   7. Wrap kết quả vào QrGenerateResult
    /// </summary>
    public class QrPayloadEngine : IQrPayloadEngine
    {
        private readonly IReadOnlyDictionary<string, IQrPayloadBuilder> _builders;

        public QrPayloadEngine(IEnumerable<IQrPayloadBuilder> builders)
        {
            _builders = builders.ToDictionary(b => b.BuilderKey, StringComparer.OrdinalIgnoreCase);
        }

        public QrGenerateResult Generate(QrGenerateRequest request)
        {
            // 1. Lookup builder
            var builderKey = BuilderKeys.From(request.ProviderCode.ToString(), request.Mode);
            if (!_builders.TryGetValue(builderKey, out var builder))
                throw new QrBuilderNotFoundException(builderKey);

            // 2. Validate
            builder.Validate(request);

            // 3. Build root payload (không có CRC)
            var payload = new StringBuilder();

            // Field 00 — Payload Format Indicator
            payload.Append(TLVEncoder.Encode(EMVTags.PayloadFormatIndicator, EMVDefaults.PayloadFormatIndicator));

            // Field 01 — Point of Initiation
            payload.Append(TLVEncoder.Encode(
                EMVTags.PointOfInitiation,
                request.IsStatic ? EMVDefaults.PointOfInitiationStatic : EMVDefaults.PointOfInitiationDynamic));

            // Field 26/27/38 — Merchant Account Info (delegated to builder)
            payload.Append(builder.BuildMerchantInfo(request));

            // Field 53 — Transaction Currency (VND = 704)
            payload.Append(TLVEncoder.Encode(EMVTags.TransactionCurrency, EMVDefaults.CurrencyVND));

            // Field 54 — Transaction Amount (optional)
            if (request.Amount.HasValue && request.Amount.Value > 0)
            {
                // EMVCo: amount là số nguyên không dấu phẩy, không có đơn vị
                var amountStr = ((long)Math.Round(request.Amount.Value)).ToString();
                payload.Append(TLVEncoder.Encode(EMVTags.TransactionAmount, amountStr));
            }

            // Field 58 — Country Code
            payload.Append(TLVEncoder.Encode(EMVTags.CountryCode, EMVDefaults.CountryVN));

            // Field 62 — Additional Data (optional description)
            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                var truncated = TLVEncoder.TruncateToByteLimit(request.Description, 99);
                var additionalData = TLVEncoder.Encode(EMVTags.AdditionalPurpose, truncated);
                payload.Append(TLVEncoder.Encode(EMVTags.AdditionalData, additionalData));
            }

            // Field 63 — CRC header (value chưa có)
            payload.Append(EMVTags.CRC + "04");

            // Tính CRC trên toàn bộ payload kể cả "6304"
            var crc = CRC16Service.Compute(payload.ToString());
            payload.Append(crc);

            var finalPayload = payload.ToString();
            var napasBin = request.NapasBinOverride
                ?? (BankBins.Exists(request.BankCode) ? BankBins.GetBin(request.BankCode) : null);

            Console.WriteLine($"[QR DEBUG] Payload: {finalPayload}");
            Console.WriteLine($"[QR DEBUG] BIN used: {napasBin}");
            Console.WriteLine($"[QR DEBUG] Account: {request.AccountNumber}");
            Console.WriteLine($"[QR DEBUG] CRC valid: {CRC16Service.Verify(finalPayload)}");

            return new QrGenerateResult
            {
                Payload = finalPayload,
                NapasBin = napasBin,
                Mode = request.Mode,
                Crc = crc,
                IsValid = true,
            };
        }

        public bool Verify(string payload) => CRC16Service.Verify(payload);
    }

}
