namespace MyWallet.Domain.Constants.Enum
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
}
