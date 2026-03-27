namespace CocoQR.Application.DTOs.Auths.Requests
{
    public class SwitchRoleReq
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
    }
}
