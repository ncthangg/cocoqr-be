using CocoQR.Domain.Constants.Enum;

namespace CocoQR.QR_Generator.Models
{

    /// <summary>
    /// Input để generate QR payload.
    /// </summary>
    public class QrGenerateRequest
    {
        public required ProviderCode ProviderCode { get; init; }
        /// <summary>BankCode tra trong BankBins (VCB, TCB, MOMO,...)</summary>
        public string? BankCode { get; init; }

        /// <summary>Số tài khoản (VietQR) hoặc SĐT (MoMo native)</summary>
        public required string AccountNumber { get; init; }

        /// <summary>Số tiền — null thì để trống (user tự nhập)</summary>
        public decimal? Amount { get; init; }

        /// <summary>Nội dung chuyển khoản — optional</summary>
        public string? Description { get; init; }

        /// <summary>
        /// Static (reusable) hay Dynamic (one-time).
        /// Default: Static cho use case lưu QR cá nhân.
        /// </summary>
        public bool IsStatic { get; init; } = true;

        /// <summary>Chế độ build payload</summary>
        public QrMode Mode { get; init; } = QrMode.VietQR;

        /// <summary>
        /// Override NapasBin — nếu null thì tự lookup từ BankCode.
        /// Chỉ cần set khi test hoặc dùng BIN custom.
        /// </summary>
        public string? NapasBinOverride { get; init; }
    }

    /// <summary>
    /// Kết quả sau khi generate payload thành công.
    /// </summary>
    public class QrGenerateResult
    {
        /// <summary>Raw EMVCo payload string — đây là thứ được encode vào QR image</summary>
        public required string Payload { get; init; }

        /// <summary>BIN đã dùng (để debug / lưu log)</summary>
        public string? NapasBin { get; init; }

        /// <summary>Mode đã dùng</summary>
        public QrMode Mode { get; init; }

        /// <summary>CRC16 checksum (4 ký tự HEX)</summary>
        public required string Crc { get; init; }

        /// <summary>True nếu payload đã pass validate CRC</summary>
        public bool IsValid { get; init; }
    }

    /// <summary>
    /// Input để render QR image.
    /// </summary>
    public class QrImageRequest
    {
        public required string Payload { get; init; }

        /// <summary>Kích thước pixel của QR image output</summary>
        public int SizePixels { get; init; } = 400;

        /// <summary>Margin (quiet zone) tính theo module</summary>
        public int Margin { get; init; } = 1;
    }

    /// <summary>
    /// Kết quả render QR image.
    /// </summary>
    public class QrImageResult
    {
        /// <summary>Raw PNG bytes</summary>
        public required byte[] PngBytes { get; init; }

        /// <summary>Base64 string (không kèm data URI prefix)</summary>
        public string Base64 => Convert.ToBase64String(PngBytes);

        /// <summary>Data URI hoàn chỉnh để dùng trong &lt;img src="..."&gt;</summary>
        public string DataUri => $"data:image/png;base64,{Base64}";
    }
}
