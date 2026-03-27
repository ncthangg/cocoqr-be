namespace CocoQR.Application.DTOs.QRStyleLibrary.Requests
{
    public class PostQRStyleReq
    {
        public string Name { get; set; } = string.Empty;
        public string StyleJson { get; set; } = string.Empty;

        public bool? IsActive { get; set; }
    }
}
