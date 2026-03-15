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
    public enum QRReceiverType
    {
        PERSONAL,
        GUEST,
    }
    public enum BankCode
    {
        VCB,
        TPB,
        SCB
    }
    public enum Currency
    {
        VND,
        USD,
        EUR
    }
}
