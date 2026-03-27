namespace CocoQR.Application.DTOs.UserRoles.Requests
{
    public class PostPutUserRoleReq
    {
        public required Guid UserId { get; set; }
        public IEnumerable<Guid> RoleIds { get; set; } = [];
    }
}
