using CocoQR.Application.DTOs.Base.BaseReq;
using CocoQR.Domain.Constants.Enum;

namespace CocoQR.Application.DTOs.Contacts.Requests
{
    public class GetContactByAdminReq : BaseAdminReq
    {
        public ContactMessageStatus? ContactStatus { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}