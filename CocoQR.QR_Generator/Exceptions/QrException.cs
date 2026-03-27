namespace CocoQR.QR_Generator.Exceptions
{

    /// <summary>Base exception cho toàn bộ CocoQR.QR domain.</summary>
    public abstract class QrException : Exception
    {
        public string Code { get; }
        protected QrException(string code, string message) : base(message) => Code = code;
    }

    /// <summary>Payload build thất bại do data không hợp lệ.</summary>
    public sealed class QrValidationException : QrException
    {
        public string Field { get; }
        public QrValidationException(string field, string message)
            : base("QR_VALIDATION_ERROR", message) => Field = field;
    }

    /// <summary>Builder chưa được đăng ký cho provider/mode combo này.</summary>
    public sealed class QrBuilderNotFoundException : QrException
    {
        public string BuilderKey { get; }
        public QrBuilderNotFoundException(string builderKey)
            : base("QR_BUILDER_NOT_FOUND",
                   $"Không tìm thấy builder cho '{builderKey}'. " +
                   $"Kiểm tra lại services.AddQrEngine() hoặc thêm builder mới.")
            => BuilderKey = builderKey;
    }

    /// <summary>CRC verify thất bại — payload bị corrupt hoặc sai chuẩn.</summary>
    public sealed class QrCrcMismatchException : QrException
    {
        public string Expected { get; }
        public string Actual { get; }
        public QrCrcMismatchException(string expected, string actual)
            : base("QR_CRC_MISMATCH",
                   $"CRC không khớp: expected={expected}, actual={actual}. Payload bị corrupt.")
        {
            Expected = expected;
            Actual = actual;
        }
    }

    /// <summary>BankCode không có trong danh sách BankBins.</summary>
    public sealed class QrBankNotFoundException : QrException
    {
        public string BankCode { get; }
        public QrBankNotFoundException(string bankCode)
            : base("QR_BANK_NOT_FOUND",
                   $"BankCode '{bankCode}' không tìm thấy trong danh sách NAPAS. " +
                   $"Thêm vào BankBins.All hoặc dùng NapasBinOverride.")
            => BankCode = bankCode;
    }
}
