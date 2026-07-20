using CocoQR.Domain.Constants.Enum;

namespace CocoQR.Application.DTOs.QRStyleLibrary.Requests
{
    public class GetQrStyleLibraryReq
    {
        public Guid? UserId { get; set; }
        public QrStyleType? Type { get; set; }
        public bool? IsActive { get; set; }
    }
}
