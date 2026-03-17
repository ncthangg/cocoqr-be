using MyWallet.Application.DTOs.Base.BaseRes;
using MyWallet.Application.DTOs.Roles.Responses;

namespace MyWallet.Application.DTOs.Users.Responses
{
    public class GetUserRes
    {
        public required Guid UserId { get; set; }
        public required string GoogleId { get; set; }
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public string PictureUrl { get; set; } = string.Empty;
        public string SecurityStamp { get; set; } = string.Empty;
    }
    public class GetUserBaseRes : BaseGetVM
    {
        public required Guid UserId { get; set; }
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public IEnumerable<GetRoleRes> GetRolesRes { get; set; } = [];
    }
}
