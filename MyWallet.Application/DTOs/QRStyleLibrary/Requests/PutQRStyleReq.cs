namespace MyWallet.Application.DTOs.QRStyleLibrary.Requests
{
    public class PutQRStyleReq
    {
        public string Name { get; set; } = string.Empty;
        public string StyleJson { get; set; } = string.Empty;

        public bool? IsActive { get; set; }
        public bool? IsDefault { get; set; }
    }
}
