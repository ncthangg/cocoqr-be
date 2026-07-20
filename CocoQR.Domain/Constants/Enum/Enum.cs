namespace CocoQR.Domain.Constants.Enum
{
    public enum RoleCategory
    {
        Admin,
        User
    }
    public enum QrStatus
    {
        Created,
        Paid,
        Expired,
        Cancelled
    }
    public enum ProviderCode
    {
        Bank,
        Momo,
        VnPay,
        ZaloPay
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
    public enum QrReceiverType
    {
        Personal,
        Guest,
    }
    public enum Currency
    {
        Vnd,
        Usd,
        Eur
    }
    public enum QrStyleType
    {
        System,
        User
    }

    public enum ContactMessageStatus
    {
        New,
        Replied,
        Ignored
    }

    public enum EmailDirection
    {
        Inbound,
        Outbound
    }

    public enum EmailDeliveryStatus
    {
        Received,
        Pending,
        Queued,
        Sending,
        Sent,
        Failed,
        Cancelled
    }

}
