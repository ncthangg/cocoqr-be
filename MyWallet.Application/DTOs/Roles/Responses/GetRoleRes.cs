using MyWallet.Application.DTOs.Base.BaseRes;

namespace MyWallet.Application.DTOs.Roles.Responses
{
    public class GetRoleRes : BaseGetVM<Guid>
    {
        public required string Name { get; set; }
        public required string NameUpperCase { get; set; }
    }
}
