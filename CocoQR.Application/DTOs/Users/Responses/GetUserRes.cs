using CocoQR.Application.DTOs.Base.BaseRes;

namespace CocoQR.Application.DTOs.Users.Responses
{
    public class GetUserRes
    {
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public string? PictureUrl { get; set; }
    }
    public class GetUserBaseRes : BaseGetVM<Guid>
    {
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public string? PictureUrl { get; set; }
        public IEnumerable<UserRoleRaw> Roles { get; set; } = [];
    }
    public class GetUserBySystemRes
    {
        public required Guid UserId { get; set; }
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public string? SecurityStamp { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? Status { get; set; }
    }
    public class UserRoleRaw
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public required string Name { get; set; }
        public required string NameUpperCase { get; set; }
    }
}
