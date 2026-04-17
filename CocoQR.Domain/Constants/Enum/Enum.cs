namespace CocoQR.Domain.Constants.Enum
{
    public enum RoleCategory
    {
        ADMIN,
        USER
    }
    public enum QRStatus
    {
        CREATED,
        PAID,
        EXPIRED,
        CANCELLED
    }
    public enum ProviderCode
    {
        BANK,
        MOMO,
        VNPAY,
        ZALOPAY
    }
    public enum QrMode
    {
        /// <summary>
        /// VietQR chuẩn (EMVCo profile) — mọi app đều quét được.
        /// Dùng NapasBIN + AccountNumber.
        /// </summary>
        VietQR,

        /// <summary>
        /// MoMo native — chỉ app MoMo quét được.
        /// Dùng số điện thoại thay vì BIN + AccountNumber.
        /// </summary>
        MomoNative,
    }
    public enum QRReceiverType
    {
        PERSONAL,
        GUEST,
    }
    public enum Currency
    {
        VND,
        USD,
        EUR
    }
    public enum QRStyleType
    {
        SYSTEM,
        USER
    }

    public enum ContactMessageStatus
    {
        NEW,
        REPLIED,
        IGNORED
    }

    public enum EmailLogStatus
    {
        PENDING,
        SUCCESS,
        FAIL
    }

    public enum EmailDirection
    {
        INCOMING,
        OUTGOING
    }

    public enum SmtpSettingType
    {
        Unknown,
        System,
        Contact,
        Admin,
        Support
    }
}
