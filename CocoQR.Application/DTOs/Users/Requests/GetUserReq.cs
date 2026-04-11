using CocoQR.Application.DTOs.Base.BaseReq;

namespace CocoQR.Application.DTOs.Users.Requests
{
    public class GetUserReq : BaseReq
    {
        public bool? Status { get; set; }
        public string? SearchValue { get; set; }
        public Guid? RoleId { get; set; }
    }
}