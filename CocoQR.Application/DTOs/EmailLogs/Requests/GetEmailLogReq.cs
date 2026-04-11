using CocoQR.Application.DTOs.Base.BaseReq;
using CocoQR.Domain.Constants.Enum;

namespace CocoQR.Application.DTOs.EmailLogs.Requests
{
    public class GetEmailLogReq : BaseReq
    {
        public SmtpSettingType? Type { get; set; }
        public EmailLogStatus? Status { get; set; }
        public EmailDirection? Direction { get; set; }
        public Guid? RecipientUserId { get; set; }
        public string? ToEmail { get; set; }
        public string? RecipientFullName { get; set; }
        public string? Subject { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}